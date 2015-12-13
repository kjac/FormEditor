using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class TextAreaField : FieldWithMandatoryValidation
	{
		public override string PrettyName
		{
			get { return "Text area"; }
		}
		public override string Type
		{
			get { return "core.textarea"; }
		}
		public int MaxLength { get; set; }

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			// let's force replace newlines with <br/> tags for multiline text
			return string.IsNullOrEmpty(value)
				? value
				: value.Replace(Environment.NewLine, "<br/>");
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
	}
}
