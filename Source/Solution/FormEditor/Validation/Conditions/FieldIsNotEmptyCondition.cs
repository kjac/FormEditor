using System.Collections.Generic;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class FieldIsNotEmptyCondition : FieldIsEmptyCondition
	{
		public override string PrettyName
		{
			get
			{
				return "Field is not empty";
			}
		}

		public override string Type
		{
			get
			{
				return "core.fieldisnotempty";
			}
		}

		public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			return !base.IsMetBy(fieldValue, allCollectedFieldValues, content);
		}

		public override string Icon
		{
			get { return DefaultIcon(base.Type); }
		}
	}
}