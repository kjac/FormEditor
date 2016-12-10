# Integrating with email marketing

In this tutorial we'll integrate Form Editor with an email marketing platform, so our editors can add a "subscribe to newsletter" checkbox to their forms.

[Campaign Monitor](https://www.campaignmonitor.com/) offers an official [.NET wrapper](http://campaignmonitor.github.io/createsend-dotnet/) for their API, which makes it an obvious choice for this tutorial. If you're more of a MailChimp kind of person, there are similar 3rd party .NET wrappers for MailChimp, and it should be pretty straight forward to apply those to the code in this tutorial.

## What to do

We'll hook into the Form Editor submission events and handle the subscription there, so our editors don't have to do anything besides adding fields to their forms. This has the (in this case) added benefit of working across all forms.

The objective is simple: If we can find a checked "subscribe to newsletter" checkbox and a valid email field on a submitted form, we'll subscribe the email address to our mailing list. Furthermore we'll attempt to dig out additional information about the end user (name, gender and zip code) and pass this along with the subscription for use within Campaign Monitor. 

## How to do it

The form submission events are described in the [documentation](../Docs/extend.md#form-submission-events). We'll add our newsletter subscription handling to the `AfterAddToIndex` event, so we only use valid form data in the newsletter subscription.

When our event handler is invoked, we'll attempt to dig out the relevant form fields by qualified quesses on their "form safe names" (basically their IDs). This means our editors need to know how to name the fields we're looking for - e.g. always name the "subscribe to newsletter" checkbox "Newsletter" or "Subscribe".

Start by adding the Campaign Monitor .NET wrapper to your project. If you're on the Visual Studio side of things, use the [NuGet](https://www.nuget.org/packages/campaignmonitor-api/) package. If you're not, you can download the NuGet package, extract *createsend-dotnet.dll* (7zip can extract NuGet packages), and dump the DLL in the */bin* folder of your site.

![Extracting NuGet package with 7zip](img/EmailMarketing/7zip nuget.png)

Next add the event handler implementation from the code listing below to your project (or you can dump it in the */App_Code* folder of your site if you're not into Visual Studio). 

You'll need to update the Campaign Monitor API key and mailing list ID in the top of the event handler to match your own Campaign Monitor account and list. If you need help finding these, have a look at the *Integrations and API* category in the *Help* section of your Campaign Monitor dashboard.

![Campaign Monitor settings](img/EmailMarketing/campaign monitor settings.png)

Here's the full code for the event handler.

```CS
﻿using System;
using System.Collections.Generic;
using System.Linq;
using createsend_dotnet;
using FormEditor;
using FormEditor.Events;
using FormEditor.Fields;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace MySite
{
	public class NewsletterSubscriptionEventHandler : ApplicationEventHandler
	{
		// Campaign Monitor configuration - you should probably put this in web.config instead
		private const string CampaignMonitorApiKey = "YOUR API KEY GOES HERE";
		private const string CampaignMonitorListId = "YOUR MAILING LIST ID GOES HERE";

		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			// add handlers for updating the article rating cache
			FormModel.AfterAddToIndex += HandleNewsletterSubscription;
		}

		private void HandleNewsletterSubscription(FormModel sender, FormEditorEventArgs formEditorEventArgs)
		{
			// get all submitted fields that hold a value
			var allSubmittedFields = sender.AllValueFields().ToArray();

			// get the "subscribe to newsletter" checkbox and make sure it's checked before signing up
			// (you can change and expand on the list of potential field names as you see it fit)
			var newsletterSubscribeField = GetField<CheckboxField>(allSubmittedFields, "Newsletter", "NewsletterSubscribe", "Subscribe", "SubscribeToNewsletter");
			if(newsletterSubscribeField == null || newsletterSubscribeField.Selected == false)
			{
				return;
			}

			// get the first available email field
			var emailField = GetField<EmailField>(allSubmittedFields);
			if(emailField == null || emailField.HasSubmittedValue == false || emailField.Invalid)
			{
				return;
			}
			// this email is valid
			var email = emailField.SubmittedValue;

			// get the value to use for "Name" on the list
			var name = GetFieldSubmittedValue(allSubmittedFields, "Name", "FullName", "FirstName");

			// get the values to use for the custom fields on the list (in this example we have a zip code and a gender field)
			var customFields = new List<SubscriberCustomField>
			{
				new SubscriberCustomField {Key = "ZipCode", Value = GetFieldSubmittedValue(allSubmittedFields, "Zip", "ZipCode", "PostalCode")},
				new SubscriberCustomField {Key = "Gender", Value = GetFieldSubmittedValue(allSubmittedFields, "Gender", "Sex")}
			};

			try
			{
				// add the email to the list
				var auth = new ApiKeyAuthenticationDetails(CampaignMonitorApiKey);
				var subscriber = new Subscriber(auth, CampaignMonitorListId);
				var result = subscriber.Add(email, name, customFields, true);
				// subscriber.Add() returns the added email address if things go well
				if(email.Equals(result, StringComparison.OrdinalIgnoreCase) == false)
				{
					LogSubscriptionError(email, name);
				}
			}
			catch(Exception ex)
			{
				LogSubscriptionError(email, name, ex);
			}
		}

		// get a field by its form safe name, using a list of candidate names to look for
		private T GetField<T>(FieldWithValue[] fields, params string[] namesToLookFor) where T : FieldWithValue
		{
			return fields.OfType<T>().FirstOrDefault(f =>
				namesToLookFor.Any() == false || namesToLookFor.Any(n =>
					// FieldHelper.FormSafeName() replaces whitespaces etc. with "_"
					// - we'll ignore that when looking for field names, so we don't have to look for both "PostalCode" and "Postal_Code"
					FieldHelper.FormSafeName(n).Replace("_", "").Equals(f.FormSafeName.Replace("_", ""), StringComparison.OrdinalIgnoreCase)
				)
			);
		}

		// get the submitted value for a field by its form safe name, using a list of candidate names to look for
		private string GetFieldSubmittedValue(FieldWithValue[] fields, params string[] namesToLookFor)
		{
			var field = GetField<FieldWithValue>(fields, namesToLookFor);
			return field != null && field.HasSubmittedValue ? field.SubmittedValue : null;
		}

		private void LogSubscriptionError(string email, string name, Exception ex = null)
		{
			LogHelper.Error<NewsletterSubscriptionEventHandler>(string.Format("Could not subscribe email: {0} (name: {1})", email, name), ex);
		}
	}
}
```

## Playtime!

To test the integration, create a form which contains:

- A text box field called "Name"
- An email field called "Email" - this should probably be made required
- A text box field called "Zip code"
- A radio button group called "Gender" with the options "Female" and "Male"
- A check box called "Subscribe"

Make sure you tick the "Subscribe" box when you submit the form. Once submitted, hopefully you'll find your email added to your Campaign Monitor list, along with any other information you filled out (if not, check the Umbraco log for any log errors).

![Campaign Monitor subscription](img/EmailMarketing/campaign monitor subscription.png)

## But wait, I have several mailing lists!

The event handler above uses one hard coded mailing list ID, which may not scale to your needs. Fortunately the `FormEditorEventArgs` exposes the Umbraco page that contains our form (as `IPublishedContent`). With this in mind you can add a picker to the document type, to let the editors pick the relevant mailing list, and access that property in the event handler. Or maybe you can figure out the correct mailing list based on the location of the Umbraco page in the content tree (by site, country etc).

Happy coding!
