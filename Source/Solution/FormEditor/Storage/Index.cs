using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;
using LuceneField = Lucene.Net.Documents.Field;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Lucene.Net.Search;
using System.Globalization;
using FormEditor.Storage.Statistics;
using Version = Lucene.Net.Util.Version;

namespace FormEditor.Storage
{
	public class Index : IIndex, IFullTextIndex, IStatisticsIndex, IUpdateIndex, IApprovalIndex
	{
		private readonly int _contentId;
		private LuceneDirectory _indexDirectory;
		private const string IdField = "_id";
		private const string CreatedField = "_created";
		private const string UpdatedField = "_updated";
		private const string ApprovalField = "_approval";
		private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

		public Index(int contentId)
		{
			_contentId = contentId;
		}

		public Guid Add(Dictionary<string, string> fields, Guid rowId)
		{
			return Add(fields, null, rowId);
		}

		public Guid Add(Dictionary<string, string> fields, Dictionary<string, IEnumerable<string>> fieldsValuesForStatistics, Guid rowId)
		{
			var created = DateTime.Now;
			return Add(fields, fieldsValuesForStatistics, rowId, created, created);
		}

		private Guid Add(Dictionary<string, string> fields, Dictionary<string, IEnumerable<string>> fieldsValuesForStatistics, Guid rowId, DateTime created, DateTime updated)
		{
			var writer = GetIndexWriter();
			Add(fields, fieldsValuesForStatistics, rowId, created, updated, writer);
			writer.Close();

			return rowId;
		}

		private void Add(Dictionary<string, string> fields, Dictionary<string, IEnumerable<string>> fieldsValuesForStatistics, Guid rowId, DateTime created, DateTime updated, IndexWriter writer)
		{
			var doc = new Document();

			doc.Add(new LuceneField(IdField, rowId.ToString(), LuceneField.Store.YES, LuceneField.Index.NOT_ANALYZED));
			doc.Add(new LuceneField(CreatedField, FormatDate(created), LuceneField.Store.YES, LuceneField.Index.NOT_ANALYZED));
			doc.Add(new LuceneField(UpdatedField, FormatDate(updated), LuceneField.Store.YES, LuceneField.Index.NOT_ANALYZED));
			doc.Add(new LuceneField(ApprovalField, ApprovalState.Undecided.ToString(), LuceneField.Store.YES, LuceneField.Index.NOT_ANALYZED));

			foreach (var field in fields)
			{
				// make sure we don't add null values
				var fieldValue = field.Value ?? string.Empty;
				doc.Add(new LuceneField(field.Key, fieldValue, LuceneField.Store.YES, LuceneField.Index.ANALYZED));

				// lo-fi sorting - just use the first 10 chars of a value for sorting
				var sortValue = fieldValue.Length > 10 ? fieldValue.Substring(0, 10) : fieldValue;
				doc.Add(new LuceneField(FieldNameForSorting(field.Key), sortValue.ToLowerInvariant(), LuceneField.Store.NO, LuceneField.Index.NOT_ANALYZED));
			}

			if (fieldsValuesForStatistics != null)
			{
				foreach (var field in fieldsValuesForStatistics)
				{
					foreach (var value in field.Value)
					{
						doc.Add(new LuceneField(FieldNameForStatistics(field.Key), value, LuceneField.Store.NO, LuceneField.Index.NOT_ANALYZED));
					}
				}
			}

			writer.AddDocument(doc);
			// optimize index for each 10 submits
			writer.Optimize(10);
		}

		public Guid Update(Dictionary<string, string> fields, Guid rowId)
		{
			return Update(fields, null, rowId);
		}

		public Guid Update(Dictionary<string, string> fields, Dictionary<string, IEnumerable<string>> fieldsValuesForStatistics, Guid rowId)
		{
			var row = Get(rowId);
			if (row == null)
			{
				// the row does not exist, add a new one
				return Add(fields, fieldsValuesForStatistics, rowId);
			}

			var created = row.CreatedDate;

			var writer = GetIndexWriter();
			Remove(new[] {rowId}, writer);
			Add(fields, fieldsValuesForStatistics, rowId, created, DateTime.Now, writer);
			writer.Close();

			return rowId;
		}

		public void Remove(IEnumerable<Guid> rowIds)
		{
			var writer = GetIndexWriter();
			Remove(rowIds, writer);
			writer.Close();
		}

		private void Remove(IEnumerable<Guid> rowIds, IndexWriter writer)
		{
			foreach (var rowId in rowIds)
			{
				RemoveFiles(rowId);
				writer.DeleteDocuments(new Term(IdField, rowId.ToString()));
				writer.Optimize();
			}
		}

		public void RemoveOlderThan(DateTime date)
		{
			var writer = GetIndexWriter();
			var query = new TermRangeQuery(UpdatedField, null, FormatDate(date), includeLower:false, includeUpper:false);

			writer.DeleteDocuments(query);
			writer.Optimize();
			writer.Close();
		}

		public Row Get(Guid rowId)
		{
			var doc = GetDocument(rowId);
			return doc == null 
				? null 
				: ParseRow(doc);
		}

		private Document GetDocument(Guid rowId)
		{
			var reader = GetIndexReader();
			var searcher = GetIndexSearcher(reader);

			var hits = searcher.Search(new TermQuery(new Term(IdField, rowId.ToString())), 1);
			Document doc = null;
			if(hits.ScoreDocs.Length > 0)
			{
				doc = searcher.Doc(hits.ScoreDocs.First().doc);
			}

			searcher.Close();
			reader.Close();

			return doc;
		}

		private static IndexSearcher GetIndexSearcher(IndexReader reader)
		{
			return new IndexSearcher(reader);
		}

		public IEnumerable<Row> Get(IEnumerable<Guid> rowIds)
		{
			var reader = GetIndexReader();
			var searcher = GetIndexSearcher(reader);

			var query = new BooleanQuery();
			foreach (var rowId in rowIds)
			{
				query.Add(new TermQuery(new Term(IdField, rowId.ToString())), BooleanClause.Occur.SHOULD);
			}

			var hits = searcher.Search(query, rowIds.Count());

			IEnumerable<Row> rows = null;
			if (hits.ScoreDocs.Length > 0)
			{
				rows = hits.ScoreDocs.Select(d => ParseRow(searcher.Doc(d.doc))).ToList();
			}

			searcher.Close();
			reader.Close();

			return rows;
		}

		public Result Get(string sortField, bool sortDescending, int count, int skip)
		{
			return GetSearchResults(null, null, sortField, sortDescending, count, skip);
		}

		public Result Get(string sortField, bool sortDescending, int count, int skip, ApprovalState approvalState)
		{
			return GetSearchResults(null, null, sortField, sortDescending, count, skip, approvalState);
		}

		public Result Search(string searchQuery, string[] searchFields, string sortField, bool sortDescending, int count, int skip)
		{
			return GetSearchResults(searchQuery, searchFields, sortField, sortDescending, count, skip);
		}

		public FieldValueFrequencyStatistics<string> GetFieldValueFrequencyStatistics(IEnumerable<string> fieldNames)
		{
			var reader = GetIndexReader();
			var result = new FieldValueFrequencyStatistics<string>(reader.NumDocs());
			foreach (var fieldName in fieldNames)
			{
				var fieldValueFrequencies = new List<FieldValueFrequency>();

				var stats = new TermRangeTermEnum(reader, FieldNameForStatistics(fieldName), null, null, true, true, null);
				if (stats.Term() != null)
				{
					do
					{
						fieldValueFrequencies.Add(new FieldValueFrequency(stats.Term().Text(), stats.DocFreq()));
					}
					while (stats.Next());
				}
				if (fieldValueFrequencies.Any())
				{
					result.Add(fieldName, fieldValueFrequencies);
				}
			}

			reader.Close();

			return result;
		}

		private Result GetSearchResults(string searchQuery, string[] searchFields, string sortField, bool sortDescending, int count, int skip, ApprovalState approvalState = ApprovalState.Any)
		{
			var reader = GetIndexReader();
			var searcher = GetIndexSearcher(reader);

			string sortFieldName;
			if(string.IsNullOrWhiteSpace(sortField))
			{
				sortField = sortFieldName = CreatedField;
				sortDescending = true;
			}
			else if(sortField == CreatedField)
			{
				sortFieldName = CreatedField;
			}
			else
			{
				sortFieldName = FieldNameForSorting(sortField);
			}

			Query query;
			if(string.IsNullOrWhiteSpace(searchQuery) == false && searchFields != null && searchFields.Any())
			{
				searchQuery = searchQuery.Replace("*", "").Replace(" ", "* ") + "*";
				var parser = new MultiFieldQueryParser(Version.LUCENE_29, searchFields, GetAnalyzer());
				parser.SetDefaultOperator(QueryParser.Operator.AND);
				try
				{
					query = parser.Parse(searchQuery.Trim());
				}
				catch(ParseException)
				{
					query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
				}
			}
			else
			{
				query = approvalState == ApprovalState.Any 
					? new MatchAllDocsQuery()
					: (Query)new TermQuery(new Term(ApprovalField, approvalState.ToString()));
			}

			var docs = searcher.Search(
				query,
				null, reader.MaxDoc(),
				new Sort(new SortField(sortFieldName, SortField.STRING, sortDescending))
				);

			var scoreDocs = docs.ScoreDocs;

			var rows = new List<Row>();
			for(var i = skip; i < (skip + count) && i < scoreDocs.Length; i++)
			{
				if(reader.IsDeleted(scoreDocs[i].doc))
				{
					continue;
				}
				var doc = searcher.Doc(scoreDocs[i].doc);
				var row = ParseRow(doc);
				rows.Add(row);
			}

			searcher.Close();
			reader.Close();

			return new Result(scoreDocs.Length, rows, sortField, sortDescending);
		}

		public bool SaveFile(HttpPostedFile file, string filename, Guid rowId)
		{
			// save uploaded file to disk
			var filesPath = PathToFiles(rowId);
			if (filesPath.Exists == false)
			{
				filesPath.Create();
			}
			file.SaveAs(Path.Combine(filesPath.FullName, filename));
			return true;
		}

		public Stream GetFile(string filename, Guid rowId)
		{
			var filesPath = PathToFiles(rowId);
			if (filesPath.Exists == false)
			{
				return null;
			}
			var filePath = Path.Combine(filesPath.FullName, filename);
			if (File.Exists(filePath) == false)
			{
				return null;
			}
			return new FileStream(filePath, FileMode.Open);
		}

		public void Delete()
		{
			var storageDirectory = PathToFormStorage();

			// step 1: delete all docs in the index to make sure it's as empty as possible in case a
			// file lock prevents us from actually deleting the index files.
			try
			{
				using(var writer = GetIndexWriter())
				{
					writer.DeleteAll();
					writer.Commit();
					writer.Close();
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex, "Could not delete all documents in index: {0}", storageDirectory.FullName);
				// don't quit here - we'll still attempt to clean up the hard way.
			}

			// step 2: explicitly delete all uploaded files (as they'll probably be taking up the most disk space).
			var filesDirectory = PathToFiles();
			if(filesDirectory.Exists)
			{
				try
				{
					filesDirectory.Delete(true);
				}
				catch(Exception ex)
				{
					Log.Error(ex, "Could not delete files directory: {0}", filesDirectory.FullName);
				}
			}

			// step 3: attempt to delete the entire index directory
			if(storageDirectory.Exists == false)
			{
				return;
			}
			try
			{
				storageDirectory.Delete(true);
			}
			catch(Exception ex)
			{
				Log.Error(ex, "Could not delete index directory: {0}", storageDirectory.FullName);
			}
		}

		public int Count()
		{
			var reader = GetIndexReader();
			var count = reader.NumDocs();
			reader.Close();
			return count;
		}

		public bool SetApprovalState(ApprovalState approvalState, Guid rowId)
		{
			var doc = GetDocument(rowId);
			if(doc == null)
			{
				return false;
			}
			doc.RemoveField(ApprovalField);
			doc.Add(new LuceneField(ApprovalField, approvalState.ToString(), LuceneField.Store.YES, LuceneField.Index.NOT_ANALYZED));

			var writer = GetIndexWriter();
			writer.UpdateDocument(new Term(IdField, rowId.ToString()), doc);
			writer.Close();

			return true;
		}

		private LuceneDirectory IndexDirectory
		{
			get
			{
				if (_indexDirectory != null)
				{
					return _indexDirectory;
				}
				_indexDirectory = FSDirectory.Open(PathToIndex());
				if (IndexReader.IndexExists(_indexDirectory) == false)
				{
					var writer = GetIndexWriter(true);
					writer.Close();
				}
				return _indexDirectory;
			}
		}

		private Row ParseRow(Document doc)
		{
			var id = Guid.Parse(doc.GetField(IdField).StringValue());
			var createdDate = DateTime.ParseExact(doc.GetField(CreatedField).StringValue(), DateTimeFormat, CultureInfo.InvariantCulture);
			var approvalStateField = doc.GetField(ApprovalField);
			var approvalState = ApprovalState.Undecided;
			if(approvalStateField != null)
			{
				Enum.TryParse(approvalStateField.StringValue(), true, out approvalState);				
			}

			var fields = new Dictionary<string, string>();
			foreach (var field in doc.GetFields().OfType<LuceneField>().Where(f => f.Name() != IdField && f.Name() != CreatedField))
			{
				fields[field.Name()] = field.StringValue();
			}
			return new Row(id, createdDate, fields, approvalState);
		}

		private static string FieldNameForSorting(string fieldName)
		{
			return $"{fieldName}_sort";
		}

		private static string FieldNameForStatistics(string fieldName)
		{
			return $"{fieldName}_stats";
		}

		private static Analyzer GetAnalyzer()
		{
			return new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
		}

		private IndexReader GetIndexReader()
		{
			return IndexReader.Open(IndexDirectory, true);
		}

		private IndexWriter GetIndexWriter(bool create = false)
		{
			return new IndexWriter(IndexDirectory, GetAnalyzer(), create, IndexWriter.MaxFieldLength.UNLIMITED);
		}

		private DirectoryInfo PathToFormStorage()
		{
			return new DirectoryInfo(HostingEnvironment.MapPath($"/App_Data/FormEditor/{_contentId}"));
		}

		private DirectoryInfo PathToIndex()
		{
			return new DirectoryInfo(Path.Combine(PathToFormStorage().FullName, "Index"));
		}

		private DirectoryInfo PathToFiles()
		{
			return new DirectoryInfo(Path.Combine(PathToFormStorage().FullName, "Files"));
		}

		private DirectoryInfo PathToFiles(Guid rowId)
		{
			return new DirectoryInfo(Path.Combine(PathToFiles().FullName, rowId.ToString("N")));
		}

		private void RemoveFiles(Guid rowId)
		{
			var filesPath = PathToFiles(rowId);
			try
			{
				if(filesPath.Exists)
				{
					filesPath.Delete(true);
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex, "Could not delete file upload directory: {0}", filesPath);
			}
		}

		private string FormatDate(DateTime date)
		{
			return date.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
		}
	}
}
