using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mail;
using FormEditor.Fields;

namespace FormEditor.Events
{
	public class FormEditorMailEventArgs : CancelEventArgs
	{
		public FormEditorMailEventArgs(MailMessage mailMessage, string emailType, IEnumerable<FieldWithValue> valueFields)
		{
			MailMessage = mailMessage;
			EmailType = emailType;
		    ValueFields = valueFields;
		}

		// the mail that's being sent
		public MailMessage MailMessage { get; private set; }

        public IEnumerable<FieldWithValue> ValueFields { get; private set; }

        // the type of mail - "confirmation" or "notification"
        public string EmailType { get; private set; }
	}
}