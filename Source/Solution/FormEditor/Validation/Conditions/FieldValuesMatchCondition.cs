using System.Collections.Generic;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class FieldValuesMatchCondition : FieldValuesDoNotMatchCondition
	{
		public override string PrettyName => "Value is equal to another field";

		public override string Type => "core.fieldvaluesmatch";

		public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			return !base.IsMetBy(fieldValue, allCollectedFieldValues, content);
		}

		public override string Icon => DefaultIcon(base.Type);
	}
}