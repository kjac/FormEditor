using System;

namespace FormEditor.ElasticIndex.Storage
{
	// type that holds the files for the form entries 
	public class File
	{
		public string Id { get; set; }
		public string Filename { get; set; }
		public byte[] Bytes { get; set; }
		public string EntryId { get; set; }
	}
}