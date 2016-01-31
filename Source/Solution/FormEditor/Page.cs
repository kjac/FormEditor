using System.Collections.Generic;
using System.Linq;
using FormEditor.Fields;

namespace FormEditor
{
	public class Page
	{
		public string Title { get; set; }
		public IEnumerable<Row> Rows { get; set; }

		public IEnumerable<Field> AllFields()
		{
			return Rows.SelectMany(r => r.Cells.SelectMany(c => c.Fields));
		}
	}
}