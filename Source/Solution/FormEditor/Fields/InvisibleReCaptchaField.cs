namespace FormEditor.Fields
{
	public class InvisibleReCaptchaField : ReCaptchaField
	{
		public override string PrettyName
		{
			get { return "Submit button (with spam protection)"; }
		}

		public override string Type
		{
			get { return "core.invisiblerecaptcha"; }
		}

		public string Name
		{
			get { return "Submit button"; }
		}

		public string FormSafeName
		{
			get { return "invisible_reCAPTCHA"; }
		}

		public string Text { get; set; }
	}
}
