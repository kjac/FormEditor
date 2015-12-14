using System.Collections.Generic;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace My.Range
{
	// we'll implement our field as a specializatin of FieldWithMandatoryValidation
	// - this gives us automatic mandatory validation for our field.
	public class MyRangeField : FieldWithMandatoryValidation
	{
		// IMPORTANT: the field must have a default constructor (or no constructor at all).

		// this is the field identifier towards Form Editor. it must be unique.
		public override string Type
		{
			get { return "my.range"; }
		}

		// this is the default field name used in the form layout (can be overridden by localization).
		public override string PrettyName
		{
			get { return "Slider"; }
		}

		// these are the custom properties for our field configuration. they must have public getters and setters.
		public int Minimum { get; set; }
		public int Maximum { get; set; }
		public int Step { get; set; }

		// custom validation of submitted values (make sure the value falls inside the configured range).
		protected override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			// this will take care of mandatory validation for us (by checking if SubmittedValue has a value).
			if(base.ValidateSubmittedValue(allCollectedValues, content) == false)
			{
				return false;
			}
			if(string.IsNullOrEmpty(SubmittedValue))
			{
				// if there's no SubmittedValue. This means mandatory must be false and nothing has been submitted
				// => we'll let the validation succeed.
				return true;
			}

			// we expect the input to be an integer for our field.
			int value;
			if(int.TryParse(SubmittedValue, out value) == false)
			{
				// for some reason a non integer value was submitted => validation fails.
				return false;
			}
			// validate that the submitted value is within the set limits.
			return value >= Minimum && value <= Maximum;
		}
	}
}