using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FormEditor.Fields;

namespace FormEditor
{
	public static class FieldHelper
	{
		private static readonly Regex FormSafeNameRegex = new Regex(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);
		// legacy form safe name regexes
		private static readonly Regex FormSafeNameRegexPriorTo43 = new Regex(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);
		private static readonly Regex FormSafeNameRegexPriorTo122 = new Regex(@"^\d*|[^a-zA-Z0-9_]", RegexOptions.Compiled);

		public static string FormSafeName(string name)
		{
			return string.Format("_{0}", FormSafeNameRegex.Replace(name ?? string.Empty, "_"));
		}

		[Obsolete("This method will be removed in an upcoming release")]
		public static string LegacyFormSafeName(string name)
		{
			return FormSafeNameRegexPriorTo43.Replace(name ?? string.Empty, "_");
		}

		internal static string GetSubmittedValue(FieldWithValue field, Dictionary<string, string> allSubmittedValues)
		{
			if(allSubmittedValues == null || allSubmittedValues.Any() == false)
			{
				return null;
			}

			if(allSubmittedValues.ContainsKey(field.FormSafeName))
			{
				return allSubmittedValues[field.FormSafeName];
			}

			// #114, #122 - support form safe name from older versions
			var legacyCandidateNames = new[]
			{
				FormSafeName(field, FormSafeNameRegexPriorTo122),
				FormSafeName(field, FormSafeNameRegexPriorTo43)
			};
			var legacyKey = allSubmittedValues.Keys.FirstOrDefault(legacyCandidateNames.Contains);
			return legacyKey != null
				? allSubmittedValues[legacyKey]
				: null;
		}

		private static string FormSafeName(FieldWithValue field, Regex regex)
		{
			return regex.Replace(field.Name ?? string.Empty, "_");			
		}
	}
}