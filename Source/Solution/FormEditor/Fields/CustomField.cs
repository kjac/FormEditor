namespace FormEditor.Fields
{
	public class CustomField : FieldWithMandatoryValidation
	{
		private readonly string _type;
		private readonly string _prettyName;

		public CustomField(string type, string prettyName)
		{
			_type = type;
			_prettyName = prettyName;
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
			get { return @"core.customfield.html"; }
		}
	}
}