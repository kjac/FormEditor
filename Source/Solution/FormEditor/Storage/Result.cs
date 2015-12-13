using System.Collections.Generic;

namespace FormEditor.Storage
{
	public class Result
	{
		public Result(int totalRows, IEnumerable<Row> rows, string sortField, bool sortDescending)
		{
			SortDescending = sortDescending;
			SortField = sortField;
			TotalRows = totalRows;
			Rows = rows;
		}
		public int TotalRows { get; private set; }
		public IEnumerable<Row> Rows { get; private set; }
		public string SortField { get; private set; }
		public bool SortDescending { get; private set; }
	}
}
