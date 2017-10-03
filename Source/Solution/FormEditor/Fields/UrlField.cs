using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class UrlField : FieldWithPlaceholder
	{
		public override string PrettyName => "Web address";

		public override string Type => "core.url";

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return FormatLink(value);
		}

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if (base.ValidateSubmittedValue(allCollectedValues, content) == false)
			{
				return false;
			}
			if (string.IsNullOrEmpty(SubmittedValue))
			{
				return true;
			}
			try
			{
				var uri = new Uri(SubmittedValue, UriKind.Absolute);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static string FormatLink(string value)
		{
			return string.IsNullOrEmpty(value)
				? value
				: $@"<a href=""{value}"" target=""_blank"">{value}</a>";
		}

		public override string SubmittedValueForEmail()
		{
			return FormatLink(SubmittedValue);
		}
	}
}
