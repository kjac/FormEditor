using System.Collections.Generic;
using FormEditor.Fields;
using FormEditor.Rendering;
using FormEditor.Validation.Conditions;
using Umbraco.Core.Models;

namespace My.Conditions
{
	public class MyLengthCondition : Condition
    {
		// IMPORTANT: the condition must have a default constructor (or no constructor at all).
		public MyLengthCondition()
		{
			// set up default values for condition configuration properties
			LessThan = 100;
		}

		// this is the condition identifier towards Form Editor. it must be unique.
		public override string Type
	    {
		    get { return "my.length"; }
	    }

		// this is the default condition name used in the validation UI (can be overridden by localization).
		public override string PrettyName
	    {
		    get { return "Text length is less than"; }
	    }

		// these are the custom properties for our condition configuration. they must have public getters and setters.
		public int LessThan { get; set; }

		// test if the condition is met by a submitted field value
	    public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
	    {
			if(fieldValue == null || fieldValue.HasSubmittedValue == false)
			{
				// no such field or no submitted field value - the condition is met (LessThan will always be >= 1)
				return false;
			}

			// the condition is met if the length of the submitted value is less than the value defined for LessThan
		    return fieldValue.SubmittedValue.Length < LessThan;
	    }

		// override this to pass custom condition configuration parameters etc. to the frontend rendering.
		// you can skip this if you do not need to pass anything (condition type is handled by the base class implementation).
		public override ConditionData ForFrontEnd()
		{
			return new FieldConditionData(this);
		}

		// custom condition configuration container for frontend rendering. you can put whatever you like in this,
		// just remember that all public properties are automatically serialized to JSON and made available to 
		// the frontend rendering.
		public class FieldConditionData : ConditionData
		{
			public FieldConditionData(MyLengthCondition condition)
				: base(condition)
			{
				// map condition configuration properties
				LessThan = condition.LessThan;
			}

			// these properties are passed to the frontend rendering
			public int LessThan { get; private set; }
		}
    }
}
