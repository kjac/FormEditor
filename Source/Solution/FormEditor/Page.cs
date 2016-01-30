using System.Collections.Generic;

namespace FormEditor
{
	public class Page
	{
		public string Title { get; set; }
		public IEnumerable<Row> Rows { get; set; }
	}
}