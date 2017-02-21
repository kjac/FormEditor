# Campaign Monitor and MailChimp integration
Form Editor has built-in support for newsletter subcription via [Campaign Monitor](https://www.campaignmonitor.com/) and [MailChimp](https://mailchimp.com/). The following describes this integration in detail.

## How it works
When the integration is configured (see below), a new field type called *Newsletter subscription* is made available to the editors. 

The *Newsletter subscription* field renders as a simple checkbox to the end users, but it behaves a bit differently behind the scenes. If the checkbox is checked when the form is submitted, the field will look for an *Email* field and subscribe the submitted email address to the newsletter list (or the first email address, if multiple emails are submitted in the *Email* field). 

## Passing additional subscriber information
Both Campaign Monitor and MailChimp have support for passing additional subscriber information in what is essentially key-value pairs. With Campaign Monitor they're called *Custom Fields*, with MailChimp it's *Merge Fields*. 

When Form Editor subscribes an email to the newsletter list, the submitted values all fields (except the *Email* field) are sent along as Custom/Merge Fields, using the field names as keys. 

Specifically for MailChimp, the keys are uppercased since MailChimp expects the keys to be in uppercase.

## Configuration
To enable the integration (and thus the *Newsletter subscription* field), you must tell Form Editor which newsletter list ID and API key to use when adding newsletter subscribers. This is done in the `<appSettings/>` of your site like this:

### Campaign Monitor configuration
```xml
<appSettings>
  <!-- ... -->
  <add key="FormEditor.CampaignMonitor.ApiKey" value="006d2181b9bd4210a1f8effface7cbd4" />
  <add key="FormEditor.CampaignMonitor.ListId" value="6726bbd5659e4e38a02e5c1ef8779181" />
</appSettings>
```

### MailChimp configuration
```xml
<appSettings>
  <!-- ... -->
  <add key="FormEditor.MailChimp.ApiKey" value="61458fd45afa4f1186fb96e665aef510-us15" />
  <add key="FormEditor.MailChimp.ListId" value="e8dcf2b9f6" />
</appSettings>
```
**Note: ** Remember to include the *data center* part of your MailChimp API key (in the sample above it's `-us15`).

## Using multiple newsletter lists
If your site uses multiple newsletter lists (e.g. one per country), you can specify the correct list ID runtime by setting the `ListId` of the *Newsletter subscription* field. One way to do this is by hooking into the `BeforeAddToIndex` event on `FormModel`:

```cs
FormModel.BeforeAddToIndex += (sender, args) =>
{
  // find the newsletter subscription field (it's base class for both 
  // the Campaign Monitor and the MailChimp newsletter subscription fields)
  var newsletterSubscriptionField = sender.AllValueFields()
    .OfType<NewsletterSubscriptionField>()
    .FirstOrDefault();
  if(newsletterSubscriptionField == null)
  {
    // no field found
    return;
  }

  // set the correct list ID to use for newsletter subscription
  newsletterSubscriptionField.ListId = "xxxxxxxx";
};
```

Event handling is described more in detail [here](extend.md#form-submission-events).

