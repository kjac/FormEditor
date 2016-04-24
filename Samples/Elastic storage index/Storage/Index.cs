using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using FormEditor.Storage;
using Nest;
using StorageRow = FormEditor.Storage.Row;

namespace FormEditor.ElasticIndex.Storage
{
	// custom Form Editor index that stores form entries to ElasticSearch.
	// this is a working sample of how a custom index could be created. you may want to tweak it to suit your needs.
	// see IIndex for the interface documentation.
	public class Index : IIndex, IFullTextIndex
	{
		private const int NotFound = 404;
		private readonly int _contentId;
		private ElasticClient _client;

		public Index(int contentId)
		{
			_contentId = contentId;
		}

		public Guid Add(Dictionary<string, string> fields, Guid rowId)
		{
			var entry = new Entry
			{
				Id = ToId(rowId),
				CreatedDate = DateTime.Now,
				FieldValues = fields
			};

			var indexResponse = Client.Index(entry);
			if (indexResponse.Created)
			{
				// the entry was successfully saved to the index. return the entry ID.
				return rowId;
			}

			LogEntryCreationError("field values", indexResponse);
			return Guid.Empty;
		}

		public void Remove(IEnumerable<Guid> rowIds)
		{
			// NOTE: DeleteByQuery is obsolete so we need to do this instead
			// get all files for this row (returning only the file ID needed for deletion) 
			var files = Client.Search<File>(s => s
				.Fields(f => f.Id)
				.Query(q =>
					q.Bool(b =>
						b.Should(
							rowIds.Select(r => q.Term(f => f.EntryId, ToId(r))).ToArray()
							)
						)
					)
				);
			foreach(var file in files.Hits)
			{
				var response = Client.Delete<File>(file.Id);
				if(response.Found == false)
				{
					LogEntryDeletionError("file", file.Id, response);
				}
			}

			foreach (var rowId in rowIds)
			{
				var response = Client.Delete<Entry>(ToId(rowId));
				if(response.Found == false)
				{
					LogEntryDeletionError("field values", ToId(rowId), response);
				}
			}
		}

		public StorageRow Get(Guid rowId)
		{
			var response = Client.Get<Entry>(ToId(rowId));
			if (response.Found == false || response.Source == null)
			{
				return null;
			}
			return ToFormRow(response.Source);
		}

		public IEnumerable<StorageRow> Get(IEnumerable<Guid> rowIds)
		{
			var ids = rowIds.Select(ToId);
			var response = Client.GetMany<Entry>(ids);

			var entries = response.Where(m => m.Found && m.Source != null).Select(m => m.Source).ToList();
			return entries.Select(ToFormRow).ToList();
		}

		public Result Get(string sortField, bool sortDescending, int count, int skip)
		{
			return GetSearchResults(null, null, sortField, sortDescending, count, skip);
		}

		public Result Search(string searchQuery, string[] searchFields, string sortField, bool sortDescending, int count, int skip)
		{
			return GetSearchResults(searchQuery, searchFields, sortField, sortDescending, count, skip);
		}

		private Result GetSearchResults(string searchQuery, string[] searchFields, string sortField, bool sortDescending, int count, int skip)
		{
			var sortFieldName = IndexFieldName(sortField);

			// special case: system fields (like created date)
			if(sortField.StartsWith("_"))
			{
				// translate system field names into something we know
				switch(sortField)
				{
					// add more cases here when applicable

					// fallback: use created date
					default:
						sortFieldName = "createdDate";
						break;
				}
			}

			// run the search.
			var response = Client.Search<Entry>(s =>
				{
					// is it a full text search?
					if(string.IsNullOrWhiteSpace(searchQuery) == false && searchFields != null && searchFields.Any())
					{
						// yup - turn the search query into a wildcard query and add it to the search descriptor
						// NOTE: this will have an impact on performance on large indexes, but we'll 
						//       assume there won't be that many form submissions in one index 
						searchQuery = searchQuery.Replace("*", "").Replace(" ", "* ") + "*";
						s.Query(q => q.QueryString(qs => qs.OnFields(searchFields.Select(IndexFieldName)).Query(searchQuery)));
					}
					return s.Sort(f =>
					{
						var sortFieldDescriptor = f.OnField(sortFieldName);
						return sortDescending ? sortFieldDescriptor.Descending() : sortFieldDescriptor.Ascending();
					}).From(skip).Take(count);
				}
			);

			if(response.ServerError == null)
			{
				// success.
				return new Result((int) response.Total, response.Documents.Select(ToFormRow), sortField, sortDescending);
			}

			// handle error
			// - 404 = index was not found. it's probably not been created yet (no entries) - that's ok.
			if(response.ServerError.Status != NotFound)
			{
				Log.Warning("An error occurred searching the index: {0}. Server error: {1} ({2}).", IndexName(), response.ServerError.Error, response.ServerError.Status);
			}
			return null;
		}

		private static string IndexFieldName(string sortField)
		{
			// our FieldValues dictionary is serialized to JSON as an object with the dictionary keys mapped to object properties.
			// therefore the index field name is fieldValues.[field name].
			return string.Format("fieldValues.{0}", sortField);
		}

		public Stream GetFile(string filename, Guid rowId)
		{
			// we based our file entry ID on the "stored filename" when saving the file. do the same when retrieving a file.
			var fileId = FilenameToFileId(filename);
			var response = Client.Get<File>(fileId);
			if (response.Found == false || response.Source == null)
			{
				return null;
			}

			return new MemoryStream(response.Source.Bytes);
		}

		public bool SaveFile(HttpPostedFile file, string filename, Guid rowId)
		{
			try
			{
				// get the file contents.
				byte[] bytes;
				using (var binaryReader = new BinaryReader(file.InputStream))
				{
					bytes = binaryReader.ReadBytes(file.ContentLength);
				}

				// the "stored filename" is unique (based on a guid), so we can base our file entry ID on it. this simplifies things when we have retrieve the file again.
				var fileData = new File
				{
					Id = FilenameToFileId(filename),
					Filename = filename,
					EntryId = ToId(rowId),
					Bytes = bytes
				};

				var indexResponse = Client.Index(fileData);
				if(indexResponse.Created)
				{
					return true;
				}

				LogEntryCreationError("file", indexResponse);
				return false;
			}
			catch (Exception ex)
			{
				Log.Warning(@"Could not save posted file ""{0}"" in index: {1}. An error occurred: {2}", filename, IndexName(), ex.Message);
				return false;
			}
		}

		public void Delete()
		{
			// create a client for the base index uri and delete the entire index by name.
			var uri = GetConnectionUri();
			var client = new ElasticClient(new ConnectionSettings(uri));
			client.DeleteIndex(IndexName());
		}

		public int Count()
		{
			var response = Client.Count<Entry>();
			return response.ServerError == null ? (int)response.Count : 0;
		}

		private static StorageRow ToFormRow(Entry entry)
		{
			return new StorageRow(Guid.Parse(entry.Id), entry.CreatedDate, entry.FieldValues);
		}

		// generate a file entry ID from the unique "stored filename" (e.g. "2ab738c3-7e15-4cd8-a23a-7b8eeb3f7d5f.upload") of a file
		private string FilenameToFileId(string filename)
		{
			return filename.Split('.').First().Replace("-", "");
		}

		private string ToId(Guid guid)
		{
			return guid.ToString("N");
		}

		private ElasticClient Client
		{
			get
			{
				if(_client == null)
				{
					// get the connection string for our Elastic index
					var uri = GetConnectionUri();
					var settings = new ConnectionSettings(uri, IndexName());
					_client = new ElasticClient(settings);

				}
				return _client;
			}
		}

		private static Uri GetConnectionUri()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["FormEditor.ElasticIndex"];
			if(connectionString == null)
			{
				throw new ConfigurationErrorsException("Connection string FormEditor.ElasticIndex was not found");
			}
			var uri = new Uri(connectionString.ConnectionString);
			return uri;
		}

		// this is the name of the index for the current Umbraco content item
		private string IndexName()
		{
			return string.Format("form-editor-{0}", _contentId);
		}

		private void LogEntryCreationError(string type, IResponse indexResponse)
		{
			Log.Warning("Could not create index {0} entry in index: {1}. {2}", type, IndexName(), FormatServerError(indexResponse));
		}

		private void LogEntryDeletionError(string type, string id, IResponse indexResponse)
		{
			Log.Warning("Could not delete index {0} entry with ID: {1} in index: {2}. {3}", type, id, IndexName(), FormatServerError(indexResponse));
		}

		private static string FormatServerError(IResponse indexResponse)
		{
			return indexResponse.ServerError != null ? string.Format("Server error: {0}", indexResponse.ServerError.Error) : "No server error available.";
		}
	}
}
