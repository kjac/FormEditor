using System.Collections.Generic;
using FormEditor.Fields;

namespace FormEditor
{
	public class Cell
	{
		public string Alias { get; set; }

		public IEnumerable<Field> Fields { get; set; }
	}
}