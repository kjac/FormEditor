using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace FormEditor.SqlIndex.Storage
{
	// table that holds the form entries 
	[TableName("FormEditorEntries")]
	[PrimaryKey("Id", autoIncrement = true)]
	public class Entry
	{
		[PrimaryKeyColumn]
		public int Id { get; set; }

		public Guid EntryId { get; set; }

		public int ContentId { get; set; }

		[Length(4000)]
		public string FieldValues { get; set; }

		public DateTime CreatedDate { get; set; }
	}
}