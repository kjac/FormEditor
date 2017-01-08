using System.Collections.Generic;
using FormEditor.Fields;

namespace FormEditor.Validation
{
	public class Action
	{
		public IEnumerable<Rule> Rules { get; set; }

		public string Task { get; set; }

		public FieldWithValue Field { get; set; }
	}
}
