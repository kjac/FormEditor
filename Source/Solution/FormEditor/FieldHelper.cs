using System.Text.RegularExpressions;

namespace FormEditor
{
	public static class FieldHelper
	{
		private static readonly Regex FormSafeNameRegex = new Regex(@"^\d*|[^a-zA-Z0-9_]", RegexOptions.Compiled);
		public static string FormSafeName(string name)
		{
			return FormSafeNameRegex.Replace(name ?? string.Empty, "_");
		}
	}
}