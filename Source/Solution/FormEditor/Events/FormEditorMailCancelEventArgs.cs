using System.ComponentModel;
using System.Net.Mail;

namespace FormEditor.Events
{
	public class FormEditorMailCancelEventArgs : CancelEventArgs
	{
		public FormEditorMailCancelEventArgs(MailMessage mailMessage, string emailType)
		{
			MailMessage = mailMessage;
			EmailType = emailType;
		}

		// the mail that's being sent
		public MailMessage MailMessage { get; private set; }

		// the type of mail - "confirmation" or "notification"
		public string EmailType { get; private set; }
	}
}