namespace FormEditor.Fields
{
	public class SubmitButtonField : Field
	{
		public override string Type => "core.submitbutton";

		public override string PrettyName => "Submit button";

		public string Text { get; set; }
	}
}