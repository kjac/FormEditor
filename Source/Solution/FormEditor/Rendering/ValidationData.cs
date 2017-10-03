using System.Collections.Generic;

namespace FormEditor.Rendering
{
	public class ValidationData
	{
		public List<RuleData> Rules { get; set; }

		public string ErrorMessage { get; set; }

		public bool Invalid { get; set; }
	}
}