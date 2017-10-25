using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using FormEditor.NewsletterSubscription;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class MailChimpSubscriptionField : NewsletterSubscriptionField
	{
		public override string Type => "core.mailchimp";

		protected override string ApiKeyAppSettingsKey => "FormEditor.MailChimp.ApiKey";

		protected override string ListIdAppSettingsKey => "FormEditor.MailChimp.ListId";

		protected override string ServiceName => "MailChimp";

		protected override void HandleSubscription(MailAddress mailAddress, IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			var valueFields = allCollectedValues.OfType<FieldWithValue>().ToArray();
			var mergeFields = valueFields
				.Except(new[] { this })
				.ToDictionary(f => f.Name, f => f.SubmittedValue);

			var api = new MailChimpApi();
			api.Subscribe(ListId, mailAddress, mergeFields, ApiKey);
		}
	}
}
