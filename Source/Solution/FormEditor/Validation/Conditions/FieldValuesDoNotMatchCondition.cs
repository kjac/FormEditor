using System;
using System.Collections.Generic;
using System.Linq;
using FormEditor.Fields;
using FormEditor.Rendering;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class FieldValuesDoNotMatchCondition : Condition 
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
			var fieldSubmittedValue = fieldValue != null && fieldValue.HasSubmittedValue ? fieldValue.SubmittedValue : string.Empty;
			var otherFieldValue = allCollectedFieldValues.FirstOrDefault(f => f.Name == OtherFieldName);
			var otherFieldSubmittedValue = otherFieldValue != null && otherFieldValue.HasSubmittedValue ? otherFieldValue.SubmittedValue : string.Empty;

			// condition is met if the two submitted values do not match
			return fieldSubmittedValue.Equals(otherFieldSubmittedValue, StringComparison.InvariantCultureIgnoreCase) == false;
		}

		public override ConditionData ForFrontEnd()
		{
			return new FieldConditionData(this);
		}

		public class FieldConditionData : ConditionData
		{
			public FieldConditionData(FieldValuesDoNotMatchCondition condition)
				: base(condition)
			{
				OtherFieldName = FieldHelper.FormSafeName(condition.OtherFieldName);
			}

			public string OtherFieldName { get; private set; }
		}
	}
}