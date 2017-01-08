using System.Collections.Generic;
using FormEditor.Fields;
using FormEditor.Validation.Conditions;
using Umbraco.Core.Models;

namespace FormEditor.Validation
{
	public class Rule
	{
		public FieldWithValue Field { get; set; }
		public Condition Condition { get; set; }

		public bool IsApplicable
		{
			get
			{
				// do not attempt to validate this rule server side if it's condition is by configuration
				return Condition.GetType() != typeof(CustomCondition);				
			}
		}

		public bool IsFulfilledBy(IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			return Condition.IsMetBy(Field, allCollectedFieldValues, content);
		}
	}
}