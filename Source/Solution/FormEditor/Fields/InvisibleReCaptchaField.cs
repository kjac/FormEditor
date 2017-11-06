namespace FormEditor.Fields
{
	public class InvisibleReCaptchaField : ReCaptchaField
	{
		public override string PrettyName => "Submit button (with spam protection)";

		public override string Type => "core.invisiblerecaptcha";

		public override string Name => "Submit button";

		public override string FormSafeName => "invisible_reCAPTCHA";

		public string Text { get; set; }
	}
}
