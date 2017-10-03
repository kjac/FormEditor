using System.Collections.Generic;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class FieldIsNotEmptyCondition : FieldIsEmptyCondition
	{
		public override string PrettyName => "Field is not empty";

		public override string Type => "core.fieldisnotempty";

		public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			return !base.IsMetBy(fieldValue, allCollectedFieldValues, content);
		}

		public override string Icon => DefaultIcon(base.Type);
	}
}