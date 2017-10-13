using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class ConsentField : FieldWithLabel, IFieldWithValidation
	{
		public override string PrettyName => "Submission consent";

		public override string Type => "core.consent";

		// Yes... this should be called Checked, but it's called Selected to be consistent 
		// with CheckboxField and the rest of the selectable fields
		public bool Selected { get; set; }

		public string ConsentText { get; set; }

		public string LinkText { get; set; }

		public int PageId { get; set; }

		public string ErrorMessage { get; set; }

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return $@"<i class=""icon icon-checkbox{(value == "true" ? string.Empty : "-empty")}""></i>";
		}

		public override string SubmittedValueForEmail()
		{
			return Selected ? "☑" : "☐";
		}

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if (base.ValidateSubmittedValue(allCollectedValues, content) == false)
			{
				return false;
			}

			Selected = string.IsNullOrEmpty(SubmittedValue) == false;

			// this field is ONLY EVER VALID if it's been checked
			return Selected;
		}
	}
}
