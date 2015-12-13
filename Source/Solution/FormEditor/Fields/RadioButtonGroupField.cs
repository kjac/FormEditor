namespace FormEditor.Fields
{
	public class RadioButtonGroupField : FieldWithFieldValues
	{
		public RadioButtonGroupField()
		{
			// default values
			FieldValues = new[] { new FieldValue { Value = "Option 1", Selected = true }, new FieldValue { Value = "Option 2" } };
		}
		public override string PrettyName
		{
			get { return "Radio button group"; }
		}
		public override string Type
		{
			get { return "core.radiobuttongroup"; }
		}
	}
}
