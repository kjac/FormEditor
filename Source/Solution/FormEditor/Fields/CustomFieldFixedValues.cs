namespace FormEditor.Fields
{
	public class CustomFieldFixedValues : FieldWithFieldValues
	{
		private readonly string _type;
		private readonly string _prettyName;

		// added for deserialization
		public CustomFieldFixedValues() { }

		public CustomFieldFixedValues(string type, string prettyName)
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
