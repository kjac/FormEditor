using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace FormEditor.Fields
{
	public class ConsentField : FieldWithLabel, IFieldWithValidation
	{
		// local cache for multiple in-request access to the Page propery
		private IPublishedContent _page;

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

		public IPublishedContent Page
		{
			get
			{
				if (PageId <= 0)
				{
					return null;
				}
				if (_page != null)
				{
					return _page;
				}
				_page = UmbracoContext.Current.ContentCache.GetById(PageId);
				return _page;
			}
		}
	}
}
