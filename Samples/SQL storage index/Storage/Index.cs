using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using FormEditor.Storage;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using StorageRow = FormEditor.Storage.Row;

namespace FormEditor.SqlIndex.Storage
{
	// custom Form Editor index that stores form entries to DB.
	// this is a working sample of how a custom index could be created. you may want to tweak it to suit your needs.
	// see IIndex for the interface documentation.
	public class Index : IIndex, IFullTextIndex, IAutomationIndex, IUpdateIndex, IApprovalIndex
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

		public Guid Update(Dictionary<string, string> fields, Guid rowId)
		{
			Remove(new[] {rowId});
			return Add(fields, rowId);
		}

		public void Remove(IEnumerable<Guid> rowIds)
		{
			foreach (var rowId in rowIds)
			{
				if(IsMsDatabaseServer(DbContext))
				{
					Database.Delete<File>("WHERE EntryId=@0", rowId);
				}
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
			return new StorageRow(entry.EntryId, entry.CreatedDate, fields, (ApprovalState)entry.Approval);
		}

		public Result Get(string sortField, bool sortDescending, int count, int skip)
		{
			return GetResult(count, skip);
		}

		public Result Get(string sortField, bool sortDescending, int count, int skip, ApprovalState approvalState)
		{
			return GetResult(count, skip, approvalState: approvalState);
		}

		public Result Search(string searchQuery, string[] searchFields, string sortField, bool sortDescending, int count, int skip)
		{
			return GetResult(count, skip, searchQuery);
		}

		private Result GetResult(int count, int skip, string query = null, ApprovalState approvalState = ApprovalState.Any)
		{
			var pageNumber = (skip / count) + 1;
			var page = GetPage(pageNumber, count, query, approvalState);

			var rows = page != null && page.Items.Any()
				? page.Items.Select(ToFormRow).Where(r => r != null).ToList()
				: new List<StorageRow>();

			return new Result(page != null ? (int)page.TotalItems : 0, rows, "Id", true);
		}

		private Page<Entry> GetPage(int pageNumber, int count, string query = null, ApprovalState approvalState = ApprovalState.Any)
		{
			// the field values were serialized into one column when they were added to the index, so we
			// can't sort on sortField. instead we'll sort on Id DESC so we always return the newest entries first.
			// full text search is likewise kinda lo-fi with a LIKE match on the serialized field values.
			return Database.Page<Entry>(pageNumber, count, 
				$"WHERE ContentId=@0 AND (@1 = '' OR FieldValues LIKE @1){(approvalState != ApprovalState.Any ? " AND Approval = @2" : "")} ORDER BY Id DESC", 
				_contentId, 
				string.IsNullOrEmpty(query) ? string.Empty : $"%{query.Trim('%')}%",
				(int)approvalState
			);
		}

		public Stream GetFile(string filename, Guid rowId)
		{
			AssertFilesUnsupported();
			var file = Database.SingleOrDefault<File>("WHERE Filename=@0", filename);
			return file == null ? null : new MemoryStream(file.Bytes);
		}

		public bool SaveFile(HttpPostedFile file, string filename, Guid rowId)
		{
			AssertFilesUnsupported();
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
			if (IsMsDatabaseServer(DbContext))
			{
				Database.Delete<File>("WHERE ContentId=@0", _contentId);
			}
			Database.Delete<Entry>("WHERE ContentId=@0", _contentId);
		}

		public int Count()
		{
			var page = GetPage(1, 1);
			return (int)page.TotalItems;
		}

		public void RemoveOlderThan(DateTime date)
		{
			Database.Delete<Entry>("WHERE ContentId=@0 AND CreatedDate<@1", _contentId, date);
		}

		public bool SetApprovalState(ApprovalState approvalState, Guid rowId)
		{
			return Database.Execute("UPDATE FormEditorEntries SET Approval = @0 WHERE EntryId = @1", (int)approvalState, rowId) > 0;
		}

		internal static void EnsureDatabase(ApplicationContext applicationContext)
		{
			var dbContext = applicationContext.DatabaseContext;
			var db = new DatabaseSchemaHelper(dbContext.Database, applicationContext.ProfilingLogger.Logger, dbContext.SqlSyntax);

			if (db.TableExist("FormEditorEntries") == false)
			{
				db.CreateTable<Entry>(false);
				// make SQL Server and Azure SQL use NVARCHAR(MAX) datatype for Entry.FieldValues
				if (IsMsDatabaseServer(dbContext))
				{
					dbContext.Database.Execute("ALTER TABLE FormEditorEntries ALTER COLUMN FieldValues NVARCHAR(MAX)");
				}
			}
			// for some reason PetaPoco fails when trying to create the File table on SQL CE - we simply won't create it then
			if (IsMsDatabaseServer(dbContext) && db.TableExist("FormEditorFiles") == false)
			{
				db.CreateTable<File>(false);
			}
		}

		private static bool IsMsDatabaseServer(DatabaseContext dbContext) => dbContext.DatabaseProvider == DatabaseProviders.SqlAzure || dbContext.DatabaseProvider == DatabaseProviders.SqlServer;

		private void AssertFilesUnsupported()
		{
			if(IsMsDatabaseServer(DbContext))
			{
				return;
			}
			throw new ApplicationException("The SQL storage index does not support files in the current database version. Try changing to MS SQL Server or Azure SQL.");
		}

		private DatabaseContext DbContext => UmbracoContext.Current.Application.DatabaseContext;

		private Database Database => DbContext.Database;
	}
}
