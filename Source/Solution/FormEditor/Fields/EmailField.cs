using System.Collections.Generic;
using System.Net.Mail;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class EmailField : FieldWithMandatoryValidation
	{
		public override string PrettyName
		{
			get { return "Email address"; }
		}
		public override string Type
		{
			get { return "core.email"; }
		}
		public bool Multiple { get; set; }

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if (base.ValidateSubmittedValue(allCollectedValues, content) == false)
			{
				return false;
			}
			if (string.IsNullOrEmpty(SubmittedValue))
			{
				return true;
			}
			var emails = Multiple ? SubmittedValue.Split(',') : new[] { SubmittedValue };
			foreach (var email in emails)
			{
				try
				{
					var address = new MailAddress(email);
				}
				catch
				{
					return false;
				}
			}
			return true;
		}
	}
}
