namespace FormEditor.Fields
{
	public class CustomFieldFixedValues : FieldWithFieldValues
	{
		public CustomFieldFixedValues(string type, string prettyName)
		{
			Type = type;
			PrettyName = prettyName;

			// default values
			FieldValues = new FieldValue[] {};
		}

		public override string Type { get; }

		public override string PrettyName { get; }

		public override string View => @"core.customfieldfixedoptions.html";
	}
}
