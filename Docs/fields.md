# Special form fields
Hopefully almost all of the form fields are self explanatory. But there are at least a few that do require a bit of introduction.

## The editorial fields
Aside from the strictly form related fields, Form Editor has a few built-in editorial fields as well. This allows the editors to mix in explanatory texts and images alongside the form fields, in order to help the end users fill out the form as best they can.

The editorial fields include:
* Heading - a single line of text.
* Text paragraph - a paragraph of non-formatted text.
* Image - an image from the Umbraco media library.
* Link - a link to a page in the Umbraco content.

## The reCAPTCHA fields
Form Editor supports [reCAPTCHA](https://www.google.com/recaptcha/) out of the box, both the "V2" and the "invisible" versions (read more about the different versions [here](https://developers.google.com/recaptcha/docs/versions)). 

* The "V2" version is added to the form as a separate field (look for this icon: ![Field icon](../Source/Umbraco/Plugin/editor/fields/core.recaptcha.png)) so the editors can position the reCAPTCHA checkbox where it fits best in their form.
* The "invisible" version is added to the form as part of a special submit button (look for this icon: ![Field icon](../Source/Umbraco/Plugin/editor/fields/core.invisiblerecaptcha.png)), since it requires no end user interaction on its own. In other words, the editors will be using this special submit button in their forms instead of the normal submit button.

*Hint:* If you don't want the editors to choose which reCAPTCHA version to use, you can use [field type groups](install.md#field-type-groups) to limit the number of fields available.

For reCAPTCHA to work you'll need to add your reCAPTCHA keys to the `<appSettings/>` of your site:
* The reCAPTCHA "site" key goes in the app setting `FormEditor.reCAPTCHA.SiteKey`
* The reCAPTCHA "secret" key goes in the app setting `FormEditor.reCAPTCHA.SecretKey`

```xml
  <appSettings>
    <!-- ... -->
    <add key="FormEditor.reCAPTCHA.SiteKey" value="****" />
    <add key="FormEditor.reCAPTCHA.SecretKey" value="****" />
  </appSettings>
```

## The member info field
When added to the form, this field automatically stores the name and email of the currently logged in member, if any. In other words there is no end user interaction required for this field. 

The field can be used in cross field validations - for example if there is no logged in member (the field is empty), an email field could be made mandatory.

## The Campaign Monitor and MailChimp subscription fields
Form Editor has built-in support for newsletter subcription via [Campaign Monitor](https://www.campaignmonitor.com/) and [MailChimp](https://mailchimp.com/). This is described in detail [here](fields_newsletter.md).

## Next step
Onwards to [reusable forms](reuse.md).
