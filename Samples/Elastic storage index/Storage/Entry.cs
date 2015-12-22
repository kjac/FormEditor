using System;
using System.Collections.Generic;

namespace FormEditor.ElasticIndex.Storage
{
	// type that holds the form entries 
	public class Entry
	{
		public string Id { get; set; }
		public Dictionary<string, string> FieldValues { get; set; }
		public DateTime CreatedDate { get; set; }
	}
}