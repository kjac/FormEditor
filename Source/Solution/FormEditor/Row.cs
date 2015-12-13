using System.Collections.Generic;

namespace FormEditor
{
	public class Row
	{
		public string Alias { get; set; }
		public IEnumerable<Cell> Cells { get; set; }
	}
}