using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class DateField : FieldWithMandatoryValidation
	{
		public override string PrettyName
		{
			get { return "Date"; }
		}
		public override string Type
		{
			get { return "core.date"; }
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
			DateTime dt;
			return DateTime.TryParse(SubmittedValue, out dt);
		}
	}
}
