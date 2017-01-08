using System.Collections.Generic;

namespace FormEditor.Rendering
{
	public class ActionData
	{
		public List<RuleData> Rules { get; set; }

		public string Task { get; set; }

		public FieldData Field { get; set; }
	}
}
