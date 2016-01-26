namespace FormEditor.Fields
{
	public abstract class FieldWithPlaceholder : FieldWithMandatoryValidation
	{
		public string Placeholder { get; set; }
	}
}