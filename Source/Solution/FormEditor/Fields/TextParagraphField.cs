namespace FormEditor.Fields
{
	public class TextParagraphField : Field
	{
		public override string Type
		{
			get { return "core.textParagraph"; }
		}

		public override string PrettyName
		{
			get { return "Text paragraph"; }
		}

		public string Text { get; set; }
	}
}