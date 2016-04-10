# Special form fields
Hopefully almost all of the form fields are self explanatory. But there are at least a few that do require a bit of introduction.

## The editorial fields
Aside from the strictly form related fields, Form Editor has a few built-in editorial fields as well. This allows the editors to mix in explanatory texts and images alongside the form fields, in order to help the end users fill out the form as best they can.

The editorial fields include:
- Heading - a single line of text.
- Text paragraph - a paragraph of non-formatted text.
- Image - an image from the Umbraco media library.
- Link - a link to a page in the Umbraco content.

## The reCAPTCHA field
Form Editor supports [reCAPTCHA](https://www.google.com/recaptcha/) out of the box. However, for it to work you'll need to add your reCAPTCHA keys to the `<appSettings/>` of your site:
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

## Next step
Onwards to [reusable forms](reuse.md).
