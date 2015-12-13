using System.Collections.Generic;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class FieldValueIsCondition : Condition, IExpectedFieldValueCondition
	{
		public string ExpectedFieldValue { get; set; }

		public override string PrettyName
		{
			get
			{
				return "Value is";
			}
		}

		public override string Type
		{
			get
			{
				return "core.fieldvalueis";
			}
		}

		public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			if (fieldValue == null)
			{
				// no such field
				return false;
			}
			return fieldValue.HasSubmittedValue && fieldValue.SubmittedValue == ExpectedFieldValue;
		}
	}
}