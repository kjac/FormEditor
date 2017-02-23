using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class CheckboxField : FieldWithMandatoryValidation, IDefaultSelectableField
	{
		public override string PrettyName
		{
			get { return "Checkbox"; }
		}
		public override string Type
		{
			get { return "core.checkbox"; }
		}
		// Yes... this should be called Checked, but it's called Selected to be consistent with 
		// FieldValue.Selected (which is used in CheckboxGroupField)
		public bool Selected { get; set; }

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return  string.Format(@"<i class=""icon icon-checkbox{0}""></i>", value == "true" ? string.Empty : "-empty");
		}

		public override string SubmittedValueForEmail()
		{
			return Selected ? "☑" : "☐";
		}

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			var valid = base.ValidateSubmittedValue(allCollectedValues, content);

			Selected = string.IsNullOrEmpty(SubmittedValue) == false;

			return valid;
		}
	}
}
