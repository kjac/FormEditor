using System.Collections.Generic;
using System.Net.Mail;
using FormEditor.NewsletterSubscription;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class MailChimpSubscriptionField : NewsletterSubscriptionField
	{
		public override string Type
		{
			get { return "core.mailchimp"; }
		}

		protected override string ApiKeyAppSettingsKey
		{
			get { return "FormEditor.MailChimp.ApiKey"; }
		}

		protected override string ListIdAppSettingsKey
		{
			get { return "FormEditor.MailChimp.ListId"; }
		}

		protected override string ServiceName
		{
			get { return "MailChimp"; }
		}

		protected override void HandleSubscription(MailAddress mailAddress, IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			var api = new MailChimpApi();
			api.Subscribe(ListId, mailAddress, ApiKey);
		}
	}
}
