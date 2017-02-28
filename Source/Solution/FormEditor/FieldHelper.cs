using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FormEditor.Fields;

namespace FormEditor
{
	public static class FieldHelper
	{
		private static readonly Regex FormSafeNameRegex = new Regex(@"^\d*|[^a-zA-Z0-9_]", RegexOptions.Compiled);
		private static readonly Regex LegacyFormSafeNameRegex = new Regex(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);

		public static string FormSafeName(string name)
		{
			return FormSafeNameRegex.Replace(name ?? string.Empty, "_");
		}

		// #114 - add handling for form safe names prior to #43
		public static string LegacyFormSafeName(string name)
		{
			return LegacyFormSafeNameRegex.Replace(name ?? string.Empty, "_");
		}

		internal static string GetSubmittedValue(FieldWithValue field, Dictionary<string, string> allSubmittedValues)
		{
			if(allSubmittedValues == null || allSubmittedValues.Any() == false)
			{
				return null;
			}

			// #114 - support form safe name from older versions
			string legacyFormSafeName;
			return allSubmittedValues.ContainsKey(field.FormSafeName)
				? allSubmittedValues[field.FormSafeName]
				: allSubmittedValues.ContainsKey((legacyFormSafeName = LegacyFormSafeName(field.Name)))
					? allSubmittedValues[legacyFormSafeName]
					: null;
		}
	}
}