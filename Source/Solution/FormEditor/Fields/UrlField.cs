using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class UrlField : FieldWithPlaceholder
	{
		public override string PrettyName
		{
			get { return "Web address"; }
		}
		public override string Type
		{
			get { return "core.url"; }
		}

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
				: string.Format(@"<a href=""{0}"" target=""_blank"">{0}</a>", value);
		}

		public override string SubmittedValueForEmail()
		{
			return FormatLink(SubmittedValue);
		}
	}
}
