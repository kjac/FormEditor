namespace FormEditor.Fields
{
	public class HeadingField : Field
	{
		public override string Type => "core.heading";

		public override string PrettyName => "Heading";

		public string Text { get; set; }
	}
}