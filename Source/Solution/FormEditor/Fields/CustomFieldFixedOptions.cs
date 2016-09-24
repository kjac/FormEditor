namespace FormEditor.Fields
{
	public class CustomFieldFixedOptions : FieldWithFieldValues
	{
		private readonly string _type;
		private readonly string _prettyName;

		public CustomFieldFixedOptions(string type, string prettyName)
		{
			_type = type;
			_prettyName = prettyName;

			// default values
			FieldValues = new FieldValue[] {};
		}

		public override string Type
		{
			get { return _type; }
		}

		public override string PrettyName
		{
			get { return _prettyName; }
		}

		public override string View
		{
			get { return @"core.customfieldfixedoptions.html"; }
		}
	}
}
