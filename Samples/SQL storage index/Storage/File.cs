using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace FormEditor.SqlIndex.Storage
{
	// table that holds the files for the form entries 
	[TableName("FormEditorFiles")]
	[PrimaryKey("Id", autoIncrement = true)]
	public class File
	{
		[PrimaryKeyColumn]
		public int Id { get; set; }

		public int ContentId { get; set; }

		public Guid EntryId { get; set; }

		public string Filename { get; set; }

		public byte[] Bytes { get; set; }
	}
}