namespace FormEditor.Fields
{
	public class HeadingField : Field
	{
		public override string Type
		{
			get { return "core.heading"; }
		}

		public override string PrettyName
		{
			get { return "Heading"; }
		}

		public string Text { get; set; }
	}
}