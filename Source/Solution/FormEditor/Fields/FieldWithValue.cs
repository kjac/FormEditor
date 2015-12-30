using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using System;

namespace FormEditor.Fields
{
	public abstract class FieldWithValue : Field
	{
		private string _formSafeName = null;

		public virtual string Name { get; set; }

		[JsonIgnore]
		public string FormSafeName
		{
			get
			{
				// use a backing field, FormSafeName will be used intensively
				if (_formSafeName == null)
				{
					_formSafeName = GetFormSafeName();
				}
				return _formSafeName;
			}
			internal set { _formSafeName = value; }
		}

		[JsonIgnore]
		public virtual string SubmittedValue { get; protected set; }

		public virtual bool HasSubmittedValue
		{
			get
			{
				return string.IsNullOrEmpty(SubmittedValue) == false;
			}
		}

		protected virtual string GetFormSafeName()
		{
			return FieldHelper.FormSafeName(Name);
		}

		protected internal override void CollectSubmittedValue(Dictionary<string, string> allSubmittedValues, IPublishedContent content)
		{
			SubmittedValue = allSubmittedValues.ContainsKey(FormSafeName)
				? allSubmittedValues[FormSafeName]
				: null;
		}

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			return true;
		}

		protected internal virtual string FormatSubmittedValueForIndex(IPublishedContent content, Guid rowId)
		{
			return SubmittedValue;
		}

		protected internal virtual string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return value;
		}

		protected internal virtual string FormatValueForCsvExport(string value, IContent content, Guid rowId)
		{
			return value;
		}

		protected internal virtual string FormatValueForFrontend(string value, IPublishedContent content, Guid rowId)
		{
			return value;
		}

		public virtual string SubmittedValueForEmail()
		{
			return SubmittedValue;
		}

		public virtual bool SupportsStripHtml
		{
			get { return true; }
		}
	}
}