using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using FormEditor.NewsletterSubscription;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public abstract class NewsletterSubscriptionField : FieldWithLabel
	{
		protected abstract string ApiKeyAppSettingsKey { get; }

		protected abstract string ListIdAppSettingsKey { get; }

		protected abstract string ServiceName { get; }

		protected abstract void HandleSubscription(MailAddress mailAddress, IEnumerable<Field> allCollectedValues, IPublishedContent content);

		public override string PrettyName
		{
			get { return "Newsletter subscription"; }
		}

		public bool Selected { get; set; }

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return string.Format(@"<i class=""icon icon-checkbox{0}""></i>", value == "true" ? string.Empty : "-empty");
		}

		public override string SubmittedValueForEmail()
		{
			return Selected ? "☑" : "☐";
		}

		protected internal override void CollectSubmittedValue(Dictionary<string, string> allSubmittedValues, IPublishedContent content)
		{
			if(HasValidConfiguration() == false)
			{
				Log.Warning("The {0} configuration is invalid. Please enter the API key in the appSetting \"{2}\" and the list ID in the appSetting \"{1}\".", ServiceName, ApiKeyAppSettingsKey, ListIdAppSettingsKey);
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

			try
			{
				HandleSubscription(mailAddress, allCollectedValues, content);
			}
			catch(Exception ex)
			{
				Log.Error(ex, "An error occurred while trying to subscribe {0} to the {1} list.", mailAddress.Address, ServiceName);
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
				return ConfigurationManager.AppSettings[ApiKeyAppSettingsKey];
			}
		}

		public string ListId
		{
			get
			{
				return ConfigurationManager.AppSettings[ListIdAppSettingsKey];
			}
		}
	}
}
