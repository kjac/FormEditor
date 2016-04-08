# Email templates
Refer to the [setup guide](install.md) for instructions on how to configure email templates for Form Editor.

An email template is just a regular razor view that renders a `FormModel`. You'll need to place your email templates in the folder */Views/Partials/FormEditor/Email/* - Form Editor ships with a [sample email template](../Source/Umbraco/Views/Partial/FormEditor/Email/EmailTemplateSample.cshtml) to help you get started. 

An email template is rendered in the Umbraco context. This means you'll be able to do all sorts of awesome stuff in your email template, because you have full access to your Umbraco data. In case you need the currently requested content item, Form Editor passes it (as `IPublishedContent`) to your view in `ViewData["currentContent"]`:

```xml
@inherits Umbraco.Web.Mvc.UmbracoViewPage<FormEditor.FormModel>
@{
  // the current content item is passed to the view as ViewData["currentContent"]
  // - Umbraco.AssignedContentItem doesn't work in this context
  var currentContent = ViewData["currentContent"] as IPublishedContent;
}
```

Have a look at the sample email template for inspiration :)

## Next step
Onwards to [special form fields](fields.md).
