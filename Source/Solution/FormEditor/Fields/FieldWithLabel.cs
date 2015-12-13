namespace FormEditor.Fields
{
	public abstract class FieldWithLabel : FieldWithValue, IFieldWithHelpText
	{
		public string Label { get; set; }
		public string HelpText { get; set; }
	}
}
