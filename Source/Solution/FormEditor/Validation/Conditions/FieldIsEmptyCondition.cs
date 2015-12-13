using System.Collections.Generic;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class FieldIsEmptyCondition : Condition
	{
		public override string PrettyName
		{
			get
			{
				return "Field is empty";
			}
		}

		public override string Type
		{
			get
			{
				return "core.fieldisempty";
			}
		}

		public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			if (fieldValue == null)
			{
				// no such field - we'll say it's empty :)
				return true;
			}
			return fieldValue.HasSubmittedValue == false;
		}
	}
}