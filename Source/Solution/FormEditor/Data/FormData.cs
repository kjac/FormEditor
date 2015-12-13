using System.Collections.Generic;

namespace FormEditor.Data
{
	public class FormData
	{
		public int TotalRows { get; set; }
		public IEnumerable<Row> Rows { get; set; }
		public string SortField { get; set; }
		public bool SortDescending { get; set; }
		public IEnumerable<Field> Fields { get; set; }
	}
}
