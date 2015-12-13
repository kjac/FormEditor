using System;
using System.Collections.Generic;
using System.Linq;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class FieldValuesDoNotMatchCondition : Condition, IFieldComparisonCondition
	{
		public string OtherFieldName { get; set; }

		public override string PrettyName
		{
			get
			{
				return "Value is not equal to another field";
			}
		}

		public override string Type
		{
			get
			{
				return "core.fieldvaluesdonotmatch";
			}
		}

		public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			if (fieldValue == null)
			{
				// no such field
				return false;
			}
			var otherFieldValue = allCollectedFieldValues.FirstOrDefault(f => f.Name == OtherFieldName);
			if (otherFieldValue == null)
			{
				// no such field
				return false;
			}
			return fieldValue.HasSubmittedValue && fieldValue.SubmittedValue.Equals(otherFieldValue.SubmittedValue, StringComparison.InvariantCultureIgnoreCase) == false;
		}

		public string GetOtherFieldFormSafeName()
		{
			return FieldHelper.FormSafeName(OtherFieldName);
		}
	}
}