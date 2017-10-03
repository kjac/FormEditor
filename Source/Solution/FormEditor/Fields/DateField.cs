using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class DateField : FieldWithMandatoryValidation
	{
		public override string PrettyName => "Date";

		public override string Type => "core.date";

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if (base.ValidateSubmittedValue(allCollectedValues, content) == false)
			{
				return false;
			}
			if (string.IsNullOrEmpty(SubmittedValue))
			{
				return Mandatory == false;
			}
			return FormatDateValue(SubmittedValue) != null;
		}

		public override string SubmittedValue
		{
			get => base.SubmittedValue;
			protected set => base.SubmittedValue = FormatDateValue(value);
		}

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return FormatDateValueForBackwardsCompatability(value);
		}

		protected internal override string FormatValueForCsvExport(string value, IContent content, Guid rowId)
		{
			return FormatDateValueForBackwardsCompatability(value);
		}

		protected internal override string FormatValueForFrontend(string value, IPublishedContent content, Guid rowId)
		{
			return FormatDateValueForBackwardsCompatability(value);
		}

		private static string FormatDateValue(string value)
		{
			if(string.IsNullOrEmpty(value))
			{
				return null;
			}
			DateTime date;
			return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date)
				? date.ToUniversalTime().ToString("yyyy-MM-dd")
				: null;
		}

		// NOTE: 
		// this provides backwards compatibility for #91. along with the consuming methods 
		// it should be removed sometime in the future
		private static string FormatDateValueForBackwardsCompatability(string value)
		{
			if(string.IsNullOrEmpty(value))
			{
				return null;
			}
			if(value.Contains("T") == false)
			{
				return value;
			}

			DateTime date;
			if(DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date) == false)
			{
				return null;
			}
			return date.ToString("yyyy-MM-dd");
		}
	}
}
