# Rendering the form
To make sure you hit the ground running, Form Editor ships with premade partial views for:

- [Synchronous form postback](../Source/Umbraco/Views/Partials/FormEditor/Sync.cshtml) using [jQuery](https://jquery.com/) to handle validation and submission.
- [Asynchronous form postback](../Source/Umbraco/Views/Partials/FormEditor/Async.cshtml) using [AngularJS](https://angularjs.org/) to handle validation and submission. 
- [Synchronous form postback](../Source/Umbraco/Views/Partials/FormEditor/NoScript.cshtml) without external script dependencies, using HTML5 to handle validation.

Once the package is installed, these partial views can be found at */Views/Partials/FormEditor/*. 

The package also installs sample templates that demonstrate how to use the partial views:

- [Synchronous form postback](../Source/Umbraco/Views/FormEditorSync.cshtml) using jQuery.
- [Asynchronous form postback](../Source/Umbraco/Views/FormEditorAsync.cshtml) using AngularJS.
- [Synchronous form postback](../Source/Umbraco/Views/FormEditorNoScript.cshtml) without external script dependencies.


## 1-2-3-done!
By completing these steps you'll have your first form rendered in no time:

1. Render the applicable partial view in your template: 
    * Use ```@Html.Partial("FormEditor/Sync", Umbraco.AssignedContentItem)``` for synchronous form postback using jQuery.
    * Use  ```@Html.Partial("FormEditor/Async", Umbraco.AssignedContentItem)``` for asynchronous form postback.
    * Use  ```@Html.Partial("FormEditor/NoScript", Umbraco.AssignedContentItem)``` for synchronous form postback without external script dependencies.

If you're using the *NoScript* rendering you don't need to do anything else. Otherwise:

2. Make sure you have included either [jQuery](https://jquery.com/) or [AngularJS](https://angularjs.org/) in your template.
3. Include the applicable Form Editor script for handling validation and form submission:
    * Use ```/JS/FormEditor/FormEditorSync.js``` for synchronous form postback.
    * Use ```/JS/FormEditor/FormEditorAsync.js``` for asynchronous form postback.

Have a look at the sample templates to see actual implementations of this.

**Note:** If your Form Editor property does not reside on the currently requested content element, you can specify the applicable content element like this: ```ViewBag.FormContent = myInstanceOfIPublishedContent;``` (see also [this page](reuse.md))

**Note:** If you have named your Form Editor property something else than "form" (see [Installing and setting up Form Editor](install.md)), you can specify the property name like this: ```ViewBag.FormName = "myForm";```

### Limitations in the *NoScript* rendering
The *NoScript* rendering provides client side validation solely by means of HTML5 validation and a bit of inline scripting to ensure that the correct error messages are shown for invalid fields. This means that the client side validation lacks support for a few things, namely:

- Cross field validation
- Required validation for *Checkbox group* fields

Of course the server side validation still works for all of the above, regardless of your choice of rendering. However, you still might want to consider [disabling the validation tab](install.md#tab-order-and-availiability) and [omitting the *Checkbox group* field type](install.md#field-type-groups) if you're using *NoScript*.

## Creating your own rendering
If you want to create your own renderings, the sample templates and partial views should always be your starting point for inspiration. They are fairly well documented and will not be discussed in detail here. However, a few things are worth mentioning.

### Rendering rows, cells and fields
As mentioned in the [setup guide](install.md), the rows and cells have aliases to help you recognize them when rendering the form. 

As for field rendering, Form Editor uses partial views to render all form fields. The partial views are referenced by convention based on the field `Type` name, and are expected to be located at:
* */Views/Partials/FormEditor/FieldsSync/* for synchronous form postback using jQuery
* */Views/Partials/FormEditor/FieldsAsync/* for asynchronous postback 
* */Views/Partials/FormEditor/FieldsNoScript/* for synchronous form postback without external script dependencies

The following code sample shows how this could all be pieced together: 

```xml
@{
  // get the form model (named "form" on the content type)
  var form = Model.Content.GetPropertyValue<FormModel>("form");
}
<div class="container">
  @foreach (var page in form.Pages)
  {
    <div class="form-page">
      @foreach (var row in page.Rows)
      {
        // use the row alias as a class, so we end up with e.g. "row one-column"
        <div class="row @row.Alias">
          @foreach (var cell in row.Cells)
          {
            // use the cell alias as a class, so we end up with e.g. "cell col-md-4"
            <div class="cell @cell.Alias">
              @foreach (var field in cell.Fields)
              {
                // render the form field using its partial view
                @Html.Partial(string.Format(@"FormEditor/FieldsSync/{0}", field.Type), field)
              }
            </div>
          }
        </div>
      }
    </div>
  }
</div>
```

### Submitting form data using synchronous postback
When using synchronous postback for form submission, simply call `CollectSubmittedValues()` on the `FormModel` property and everything (including redirects to the success page, if configured) will be handled for you: 

```cs
// get the form model (named "form" on the content type)
var form = Model.Content.GetPropertyValue<FormModel>("form");
// collect submitted values and redirect (this does nothing unless it's a postback)
form.CollectSubmittedValues();
```

If you need to work with the submitted values after the postback, you can retrieve the ID of the submission from the `RowId` property on the form model:

```cs
// get the form model (named "form" on the content type)
var form = Model.Content.GetPropertyValue<FormModel>("form");
// collect submitted values but do not redirect
if (form.CollectSubmittedValues(redirect: false))
{
  // get the ID of the submission
  var id = form.RowId;
  // do something with the ID - for example redirect to the recipt page with the ID in the query string
  if (form.SuccessPageId > 0)
  {
    var successPage = Umbraco.TypedContent(form.SuccessPageId);
    if (successPage != null)
    {
      HttpContext.Current.Response.Redirect(successPage.Url + "?id=" + id);
    }
  }
}
```

### Submitting form data using asynchronous postback
When using asynchronous postback for form submission you'll need to create a `FormData` object, populate it with the form data you want to submit and POST it to the Form Editor `SubmitEntry` endpoint at */umbraco/FormEditorApi/Public/SubmitEntry/* - like this: 

```javascript
// create a form data container
var data = new FormData();
// add the form data by using data.append(key, value) 
data.append("Text_box", "some value");
// post it to the Form Editor SubmitEntry endpoint
$http.post("/umbraco/FormEditorApi/Public/SubmitEntry/", data, { headers: { "Content-Type": undefined } }).then(/* handle response here */);
```

Please remember the `"Content-Type": undefined` header, otherwise stuff won't work.

The response from the endpoint contains the receipt page URL (if a receipt page is configured) and the ID of the submission.

## Next step
Onwards to [Email templates](emails.md).
