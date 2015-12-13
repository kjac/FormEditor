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
using Lucene.Net.Store;
using LuceneField = Lucene.Net.Documents.Field;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Lucene.Net.Search;
using System.Globalization;

namespace FormEditor.Storage
{
	public class Index : IIndex
	{
		private readonly int _contentId;
		private LuceneDirectory _indexDirectory;
		private const string IdField = "_id";
		private const string CreatedField = "_created";
		private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

		public Index(int contentId)
		{
			_contentId = contentId;
		}

		public Guid Add(Dictionary<string, string> fields)
		{
			var doc = new Document();

			var id = Guid.NewGuid();
			doc.Add(new LuceneField(IdField, id.ToString(), LuceneField.Store.YES, LuceneField.Index.NOT_ANALYZED));
			doc.Add(new LuceneField(CreatedField, DateTime.Now.ToString(DateTimeFormat, CultureInfo.InvariantCulture), LuceneField.Store.YES, LuceneField.Index.NOT_ANALYZED));

			foreach (var field in fields)
			{
				// make sure we don't add null values
				var fieldValue = field.Value ?? string.Empty;
				doc.Add(new LuceneField(field.Key, fieldValue, LuceneField.Store.YES, LuceneField.Index.ANALYZED));

				// lo-fi sorting - just use the first 10 chars of a value for sorting
				var sortValue = fieldValue.Length > 10 ? fieldValue.Substring(0, 10) : fieldValue;
				doc.Add(new LuceneField(FieldNameForSorting(field.Key), sortValue.ToLowerInvariant(), LuceneField.Store.NO, LuceneField.Index.NOT_ANALYZED));
			}

			var writer = GetIndexWriter();
			writer.AddDocument(doc);
			// optimize index for each 10 submits
			writer.Optimize(10);
			writer.Close();

			return id;
		}

		public void Remove(IEnumerable<Guid> rowIds)
		{
			var writer = GetIndexWriter();
			foreach (var rowId in rowIds)
			{
				writer.DeleteDocuments(new Term(IdField, rowId.ToString()));
			}
			writer.Close();
		}

		public Row Get(Guid rowId)
		{
			var reader = IndexReader.Open(IndexDirectory, true);
			var searcher = new IndexSearcher(reader);

			var hits = searcher.Search(new TermQuery(new Term(IdField, rowId.ToString())), 1);
			if(hits.ScoreDocs.Length == 0)
			{
				return null;
			}
			var doc = searcher.Doc(hits.ScoreDocs.First().doc);
			return ParseRow(doc);
		}

		public IEnumerable<Row> Get(IEnumerable<Guid> rowIds)
		{
			var reader = IndexReader.Open(IndexDirectory, true);
			var searcher = new IndexSearcher(reader);

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
			var reader = IndexReader.Open(IndexDirectory, true);
			var searcher = new IndexSearcher(reader);

			string sortFieldName;
			if (string.IsNullOrWhiteSpace(sortField))
			{
				sortField = sortFieldName = CreatedField;
				sortDescending = true;
			}
			else if (sortField == CreatedField)
			{
				sortFieldName = CreatedField;
			}
			else
			{
				sortFieldName = FieldNameForSorting(sortField);
			}

			var docs = searcher.Search(
				new MatchAllDocsQuery(),
				null, reader.MaxDoc(),
				new Sort(new SortField(sortFieldName, SortField.STRING, sortDescending))
				);

			var scoreDocs = docs.ScoreDocs;

			var rows = new List<Row>();
			for (var i = skip; i < (skip + count) && i < scoreDocs.Length; i++)
			{
				if (reader.IsDeleted(scoreDocs[i].doc))
				{
					continue;
				}
				var doc = searcher.Doc(scoreDocs[i].doc);
				var row = ParseRow(doc);
				rows.Add(row);
			}

			searcher.Close();
			reader.Close();

			return new Result(scoreDocs.Count(), rows, sortField, sortDescending);
		}

		public bool SaveFile(HttpPostedFile file, string filename)
		{
			// save uploaded file to disk
			var filesPath = PathToFiles();
			if (filesPath.Exists == false)
			{
				filesPath.Create();
			}
			file.SaveAs(Path.Combine(filesPath.FullName, filename));
			return true;
		}

		public Stream GetFile(string filename)
		{
			var filesPath = PathToFiles();
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
			var fields = new Dictionary<string, string>();
			foreach (var field in doc.GetFields().OfType<LuceneField>().Where(f => f.Name() != IdField && f.Name() != CreatedField))
			{
				fields[field.Name()] = field.StringValue();
			}
			return new Row(id, createdDate, fields);
		}

		private static string FieldNameForSorting(string fieldName)
		{
			return string.Format("{0}_sort", fieldName);
		}

		private static Analyzer GetAnalyzer()
		{
			return new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
		}

		private IndexWriter GetIndexWriter(bool create = false)
		{
			return new IndexWriter(IndexDirectory, GetAnalyzer(), create, IndexWriter.MaxFieldLength.UNLIMITED);
		}

		private DirectoryInfo PathToFormStorage()
		{
			return new DirectoryInfo(HostingEnvironment.MapPath(string.Format("/App_Data/FormEditor/{0}", _contentId)));
		}

		private DirectoryInfo PathToIndex()
		{
			return new DirectoryInfo(Path.Combine(PathToFormStorage().FullName, "Index"));
		}

		private DirectoryInfo PathToFiles()
		{
			return new DirectoryInfo(Path.Combine(PathToFormStorage().FullName, "Files"));
		}
	}
}
