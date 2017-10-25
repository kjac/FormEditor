namespace FormEditor.Fields
{
	public class CheckboxGroupField : FieldWithFieldValues
	{
		public CheckboxGroupField()
		{
			// default values
			FieldValues = new[] { new FieldValue { Value = "Option 1", Selected = true }, new FieldValue { Value = "Option 2" } };
		}
		public override string PrettyName => "Checkbox group";

		public override string Type => "core.checkboxgroup";

		public override bool IsMultiSelectEnabled => true;
	}
}
