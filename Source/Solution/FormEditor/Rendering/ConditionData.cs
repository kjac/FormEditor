using FormEditor.Validation.Conditions;

namespace FormEditor.Rendering
{
	public class ConditionData
	{
		internal ConditionData()
		{
		}

		public ConditionData(Condition condition)
		{
			Type = condition.Type;
		}

		public string Type { get; }
	}
}