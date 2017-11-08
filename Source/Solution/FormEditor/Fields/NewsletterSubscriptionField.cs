using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public abstract class NewsletterSubscriptionField : FieldWithLabel, IDefaultSelectableField
	{
		private string _listId;

		protected abstract string ApiKeyAppSettingsKey { get; }

		protected abstract string ListIdAppSettingsKey { get; }

		protected abstract string ServiceName { get; }

		protected abstract void HandleSubscription(MailAddress mailAddress, IEnumerable<Field> allCollectedValues, IPublishedContent content);

		public override string PrettyName => "Newsletter subscription";

		public bool Selected { get; set; }

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return $@"<i class=""icon icon-checkbox{(value == "true" ? string.Empty : "-empty")}""></i>";
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

		protected internal override void AfterAddToIndex(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if(Selected == false)
			{
				return;
			}

			var emailField = allCollectedValues.OfType<EmailField>().FirstOrDefault();
			var mailAddress = emailField?.GetSubmittedMailAddresses().FirstOrDefault();
			if(mailAddress == null)
			{
				return;
			}

			try
			{
				HandleSubscription(mailAddress, allCollectedValues, content);
			}
			catch(Exception ex)
			{
				// swallow and log any exception that occurred during subscription
				Log.Error(ex, "An error occurred while trying to subscribe {0} to the {1} list.", mailAddress.Address, ServiceName);
			}
		}

		public override bool CanBeAddedToForm => HasValidConfiguration();

		private bool HasValidConfiguration()
		{
			return string.IsNullOrEmpty(ApiKey) == false
				   && string.IsNullOrEmpty(ListId) == false;
		}

		[JsonIgnore]
		public string ApiKey => ConfigurationManager.AppSettings[ApiKeyAppSettingsKey];

		[JsonIgnore]
		// we're using a backing property here to allow custom code to set a different list ID
		public string ListId
		{
			get
			{
				if(_listId == null)
				{
					_listId = ConfigurationManager.AppSettings[ListIdAppSettingsKey];
				}
				return _listId;
			}
			set => _listId = value;
		}
	}
}
