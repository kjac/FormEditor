using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class NumberField : FieldWithMandatoryValidation
	{
		public NumberField()
		{
			// default values
			Min = 0;
			Max = 100;
		}
		public override string PrettyName
		{
			get { return "Number"; }
		}
		public override string Type
		{
			get { return "core.number"; }
		}
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
			int temp;
			if (int.TryParse(SubmittedValue, out temp) == false)
			{
				return false;
			}
			return temp >= Min && temp <= Max;
		}
	}
}
