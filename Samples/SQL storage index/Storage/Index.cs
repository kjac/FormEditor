using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using FormEditor.Storage;
using Newtonsoft.Json;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using StorageRow = FormEditor.Storage.Row;

namespace FormEditor.SqlIndex.Storage
{
	// custom Form Editor index that stores form entries to DB.
	// this is a working sample of how a custom index could be created. you may want to tweak it to suit your needs.
	// see IIndex for the interface documentation.
	public class Index : IIndex
	{
		private readonly int _contentId;

		public Index(int contentId)
		{
			_contentId = contentId;
		}

		public Guid Add(Dictionary<string, string> fields, Guid rowId)
		{
			// for simplicity this index serializes the field values into one column. 
			// unfortunately this means we can't sort by field value when getting the
			// form entries, but we'll just have to live with that.
			var row = new Entry
			{
				ContentId = _contentId,
				CreatedDate = DateTime.Now,
				FieldValues = JsonConvert.SerializeObject(fields),
				EntryId = rowId
			};
			Database.Insert(row);
			return row.EntryId;
		}

		public void Remove(IEnumerable<Guid> rowIds)
		{
			foreach (var rowId in rowIds)
			{
				Database.Delete<File>("WHERE EntryId=@0", rowId);
				Database.Delete<Entry>("WHERE EntryId=@0", rowId);
			}
		}

		public StorageRow Get(Guid rowId)
		{
			var row = Database.SingleOrDefault<Entry>("WHERE EntryId=@0", rowId);
			if (row == null)
			{
				return null;
			}

			return ToFormRow(row);
		}

		public IEnumerable<StorageRow> Get(IEnumerable<Guid> rowIds)
		{
			var rows = Database.Query<Entry>("WHERE EntryId IN (@0)", rowIds).ToList();
			if (rows.Any() == false)
			{
				return null;
			}
			return rows.Select(ToFormRow).ToList();
		}

		private static StorageRow ToFormRow(Entry entry)
		{
			var fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry.FieldValues);
			return new StorageRow(entry.EntryId, entry.CreatedDate, fields);
		}

		public Result Get(string sortField, bool sortDescending, int count, int skip)
		{
			// the field values were serialized into one column when they were added to the index, so we
			// can't sort on sortField. instead we'll sort on Id DESC so we always return the newest entries first.
			var pageNumber = (skip / count) + 1;
			var page = GetPage(pageNumber, count);

			var rows = page != null && page.Items.Any()
				? page.Items.Select(ToFormRow).Where(r => r != null).ToList()
				: new List<StorageRow>();

			return new Result(page != null ? (int)page.TotalItems : 0, rows, "Id", true);
		}

		private Page<Entry> GetPage(int pageNumber, int count)
		{
			return Database.Page<Entry>(pageNumber, count, "WHERE ContentId=@0 ORDER BY Id DESC", _contentId);
		}

		public Stream GetFile(string filename, Guid rowId)
		{
			var file = Database.SingleOrDefault<File>("WHERE Filename=@0", filename);
			return file == null ? null : new MemoryStream(file.Bytes);
		}

		public bool SaveFile(HttpPostedFile file, string filename, Guid rowId)
		{
			try
			{
				var bytes = new byte[file.ContentLength];
				file.InputStream.Read(bytes, 0, file.ContentLength);
				// #86 - make sure we reset the stream position to the beginning of the file
				file.InputStream.Seek(0, SeekOrigin.Begin);

				Database.Insert(
					new File
					{
						ContentId = _contentId,
						EntryId = rowId,
						Filename = filename,
						Bytes = bytes
					}
				);

				return true;
			}
			catch (Exception ex)
			{
				Log.Warning(@"Could not save posted file ""{0}"" - an error occurred: {1}", filename, ex.Message);
				return false;
			}
		}

		public void Delete()
		{
			Database.Delete<File>("WHERE ContentId=@0", _contentId);
			Database.Delete<Entry>("WHERE ContentId=@0", _contentId);
		}

		public int Count()
		{
			var page = GetPage(1, 1);
			return (int)page.TotalItems;
		}

		private Database Database => UmbracoContext.Current.Application.DatabaseContext.Database;
	}
}
