namespace FormEditor.Fields
{
	public class TelephoneField : FieldWithPlaceholder
	{
		public override string PrettyName
		{
			get { return "Phone"; }
		}
		public override string Type
		{
			get { return "core.telephone"; }
		}
	}
}
