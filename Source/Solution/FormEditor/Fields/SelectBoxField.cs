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

		public override string PrettyName => "Select box (multiple)";

		public override string Type => "core.selectbox";

		public bool MultiSelect { get; set; }

		public override bool IsMultiSelectEnabled => MultiSelect;

		public int? Size { get; set; }

		// force the statistics graphs to view this field type as multivalue, as it might 
		// have been at one time or another (it can be toggled on and off at will)
		public override bool MultipleValuesPerEntry => true;
	}
}
