namespace FormEditor.Fields
{
	public class SelectBoxField : FieldWithFieldValues
	{
		public SelectBoxField()
		{
			// default values
			FieldValues = new[] { new FieldValue { Value = "Value 1" }, new FieldValue { Value = "Value 2" } };
			MultiSelect = true;
		}
		public override string PrettyName
		{
			get { return "Select box"; }
		}
		public override string Type
		{
			get { return "core.selectbox"; }
		}
		public bool MultiSelect { get; set; }
		public override bool IsMultiSelectEnabled
		{
			get { return MultiSelect; }
		}
	}
}
