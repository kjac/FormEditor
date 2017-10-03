using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public abstract class FieldWithMandatoryValidation : FieldWithLabel, IFieldWithValidation
	{
		public bool Mandatory { get; set; }

		public string ErrorMessage { get; set; }

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if (base.ValidateSubmittedValue(allCollectedValues, content) == false)
			{
				return false;
			}
			return Mandatory == false || string.IsNullOrWhiteSpace(SubmittedValue) == false;
		}
	}
}
