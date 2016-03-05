# Reusable forms
Form Editor is meant to be used just like any other Umbraco property editor: In the context of one specific page. However, in some cases you might want to reuse a form across multiple pages, and of course you can do that as well. 

## Setting it all up
First of all you need a form and some pages that will contain the form. Usually you'll create the form in a content folder outside of your site structure - something like this:

```
- Front Page
    - Page 01
    - Page 02
- Forms Folder
    - My Reusable Form
```

Now add a content picker to the page content type and select *My Reusable Form* on the pages where you want to display the form. In the following paragraphs the content picker is assumed to have the alias ```formContentPicker```.

## Tweaking the templates
Once you've got the content all set up, you'll need to tweak the template a bit in order to:

1. Retrieve the form model from the selected form.
2. Store the form submissions on the selected form.

The "selected form" in this case is *My Reusable Form*.

### For synchronous form postback
Start out with the [sample template for synchronous form postback](../Source/Umbraco/Views/FormEditorSync.cshtml). If you replace the form model retrieval and form submission handling with the following lines, you're good to go:

```cs
// get the selected content that contains the form model 
var formContentId = Model.Content.GetPropertyValue<int>("formContentPicker");
var formContent = Umbraco.TypedContent(formContentId);

// get the form model (named "form" on the content type)
var form = formContent.GetPropertyValue<FormModel>("form");

// handle form submission in case of a postback
// - the form submission will be stored on the content that contains the form model
var formWasSubmitted = form.CollectSubmittedValues(formContent);
```

## For asynchronous form postback
Start out with the [sample template for asynchronous form postback](../Source/Umbraco/Views/FormEditorAsync.cshtml) and replace the form retrieval with the following lines:

```cs
// get the selected content that contains the form model 
var formContentId = Model.Content.GetPropertyValue<int>("formContentPicker");
var formContent = Umbraco.TypedContent(formContentId);
```

That's it - you're done. 

## Next step
Onwards to [working with form submissions](submissions.md).