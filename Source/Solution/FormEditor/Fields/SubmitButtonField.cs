namespace FormEditor.Fields
{
	public class SubmitButtonField : Field
	{
		public override string Type
		{
			get { return "core.submitbutton"; }
		}

		public override string PrettyName
		{
			get { return "Submit button"; }
		}

		public string Text { get; set; }
	}
}