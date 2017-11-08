using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class NumberField : FieldWithPlaceholder
	{
		public NumberField()
		{
			// default values
			Min = 0;
			Max = 100;
		}
		public override string PrettyName => "Number";

		public override string Type => "core.number";

		public int Min { get; set; }

		public int Max { get; set; }

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
			if (int.TryParse(SubmittedValue, out var toValidate) == false)
			{
				return false;
			}
			return toValidate >= Min && toValidate <= Max;
		}
	}
}
