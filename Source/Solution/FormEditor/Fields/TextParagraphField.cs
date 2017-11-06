namespace FormEditor.Fields
{
	public class TextParagraphField : Field
	{
		public override string Type => "core.textParagraph";

		public override string PrettyName => "Text paragraph";

		public string Text { get; set; }
	}
}