using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class TextAreaField : FieldWithPlaceholder
	{
		public override string PrettyName => "Text area";

		public override string Type => "core.textarea";

		public int MaxLength { get; set; }

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return ReplaceNewLines(value);
		}

		public override string SubmittedValueForEmail()
		{
			return ReplaceNewLines(SubmittedValue);
		}

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if (base.ValidateSubmittedValue(allCollectedValues, content) == false)
			{
				return false;
			}
			if (MaxLength <= 0)
			{
				return true;
			}
			return string.IsNullOrEmpty(SubmittedValue) || SubmittedValue.Length <= MaxLength;
		}

		private static string ReplaceNewLines(string value)
		{
			// replace newlines with <br/> tags for multiline text
			return string.IsNullOrEmpty(value)
				? value
				: value.Replace(Environment.NewLine, "<br/>");
		}
	}
}
