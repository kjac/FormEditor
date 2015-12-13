using System;
using System.Collections.Generic;

namespace FormEditor.Storage
{
	public class Row
	{
		public Row(Guid id, DateTime createdDate, Dictionary<string, string> fields)
		{
			Id = id;
			Fields = fields;
			CreatedDate = createdDate;
		}
		public Guid Id { get; private set; }
		public DateTime CreatedDate { get; private set; }
		public Dictionary<string, string> Fields { get; private set; }
	}
}
