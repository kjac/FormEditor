using System.Collections.Generic;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class FieldValueIsNotCondition : FieldValueIsCondition
	{
		public override string PrettyName => "Value is not";

		public override string Type => "core.fieldvalueisnot";

		public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			return !base.IsMetBy(fieldValue, allCollectedFieldValues, content);
		}

		public override string Icon => DefaultIcon(base.Type);
	}
}