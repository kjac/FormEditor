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

Now add a content picker to the page content type and select *My Reusable Form* on the pages where you want to display the form. In the following, the content picker property is assumed to have the alias ```formContentPicker```.

## Tweaking the templates
Once you've got the content all set up, you'll need to tell the Form Editor to use selected content (*My Reusable Form*) instead of the currently requested content. 

The sample templates for [synchronous](../Source/Umbraco/Views/FormEditorSync.cshtml) and [asynchronous](../Source/Umbraco/Views/FormEditorAsync.cshtml) form postback have already been prepared for this. All you need to do is assign the selected content to ```ViewBag.FormContent``` - the sample templates will do the rest.

```cs
// get the selected content that contains the form model 
var formContentId = Model.Content.GetPropertyValue<int>("formContentPicker");
var formContent = Umbraco.TypedContent(formContentId);

// assign the selected content to ViewBag.FormContent
ViewBag.FormContent = formContent;
```

That's it - you're good to go. 

## Next step
Onwards to [working with form submissions](submissions.md).