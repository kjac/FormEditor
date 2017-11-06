namespace FormEditor.Fields
{
	public class CustomField : FieldWithMandatoryValidation
	{
		public CustomField(string type, string prettyName)
		{
			Type = type;
			PrettyName = prettyName;
		}

		public override string Type { get; }

		public override string PrettyName { get; }

		public override string View => @"core.customfield.html";
	}
}