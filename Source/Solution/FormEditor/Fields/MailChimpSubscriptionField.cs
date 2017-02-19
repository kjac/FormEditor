using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FormEditor.NewsletterSubscription;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class MailChimpSubscriptionField : FieldWithLabel
	{
		const string ApiKeyAppSetting = "FormEditor.MailChimp.ApiKey";
		const string ListIdAppSetting = "FormEditor.MailChimp.ListId";

		public override string PrettyName
		{
			get { return "Newsletter subscription"; }
		}
		public override string Type
		{
			get { return "core.mailchimp"; }
		}

		public bool Selected { get; set; }

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return  string.Format(@"<i class=""icon icon-checkbox{0}""></i>", value == "true" ? string.Empty : "-empty");
		}

		public override string SubmittedValueForEmail()
		{
			return Selected ? "☑" : "☐";
		}

		protected internal override void CollectSubmittedValue(Dictionary<string, string> allSubmittedValues, IPublishedContent content)
		{
			if(HasValidConfiguration() == false)
			{
				Log.Warning("The MailChimp configuration is invalid. Please enter the API key in the appSetting \"{0}\" and the list ID in the appSetting \"{1}\".", ApiKeyAppSetting, ListIdAppSetting);
				return;
			}

			base.CollectSubmittedValue(allSubmittedValues, content);
			Selected = string.IsNullOrEmpty(SubmittedValue) == false;
		}

		// NOTE: we're not going to fail the form validation just because the newsletter signup didn't succeed
		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if(Selected == false)
			{
				return true;
			}

			var emailField = allCollectedValues.OfType<EmailField>().FirstOrDefault();
			var mailAddress = emailField != null ? emailField.GetSubmittedMailAddresses().FirstOrDefault() : null;
			if(mailAddress == null)
			{
				return true;
			}

			var api = new MailChimpApi();
			try
			{
				api.Subscribe(ListId, mailAddress, ApiKey);
			}
			catch(Exception ex)
			{
				Log.Error(ex, "An error occurred while trying to subscribe {0} to the MailChimp list.", mailAddress.Address);
			}

			return true;
		}

		public override bool CanBeAddedToForm
		{
			get
			{
				return HasValidConfiguration();
			}
		}

		private bool HasValidConfiguration()
		{
			return string.IsNullOrEmpty(ApiKey) == false
				   && string.IsNullOrEmpty(ListId) == false;
		}

		public string ApiKey
		{
			get
			{
				return ConfigurationManager.AppSettings[ApiKeyAppSetting];
			}
		}

		public string ListId
		{
			get
			{
				return ConfigurationManager.AppSettings[ListIdAppSetting];
			}
		}
	}
}
