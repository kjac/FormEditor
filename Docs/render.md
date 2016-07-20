# Rendering the form
To make sure you hit the ground running, Form Editor ships with premade partial views for [synchronous form postback](../Source/Umbraco/Views/Partials/FormEditor/Sync.cshtml) (using [jQuery](https://jquery.com/)) and [asynchronous form postback](../Source/Umbraco/Views/Partials/FormEditor/Async.cshtml) (using [AngularJS](https://angularjs.org/)). Once the package is installed, these partial views can be found at */Views/Partials/FormEditor/*. 

The package also installs two sample templates that demonstrate how to use the partial views - one that demonstrates [synchronous form postback](../Source/Umbraco/Views/FormEditorSync.cshtml) and one that demonstrates [asynchronous form postback](../Source/Umbraco/Views/FormEditorAsync.cshtml).

## 1-2-3-done!
By completing these three steps, you'll have your first form rendered in no time:

1. Render the applicable partial view in your template: 
    * Use ```@Html.Partial("FormEditor/Sync", Umbraco.AssignedContentItem);``` for synchronous form postback.
    * Use  ```@Html.Partial("FormEditor/Async", Umbraco.AssignedContentItem);``` for asynchronous form postback.
2. Make sure you have included either [jQuery](https://jquery.com/) or [AngularJS](https://angularjs.org/) in your template (even if you're using synchronous form postback, you'll still want scripting support for client side validation).
3. Include the applicable Form Editor script for handling validation and form submission:
    * Use ```/JS/FormEditor/FormEditorSync.js``` for synchronous form postback.
    * Use ```/JS/FormEditor/FormEditorAsync.js``` for asynchronous form postback.

Have a look at the sample templates to see actual implementations of this.

**Note:** If you have named your Form Editor property something else than "form" (see [Installing and setting up Form Editor](install.md)), you can specify the property name like this: ```ViewBag.FormName = "myForm";```

## Creating your own rendering
If you want to create your own renderings, the sample templates and partial views should always be your starting point for inspiration. They are fairly well documented and will not be discussed in detail here. However, a few things are worth mentioning.

### Rendering rows, cells and fields
As mentioned in the [setup guide](install.md), the rows and cells have aliases to help you recognize them when rendering the form. 

As for field rendering, Form Editor uses partial views to render all form fields. The partial views are referenced by convention based on the field `Type` name, and are expected to be located at:
* */Views/Partials/FormEditor/FieldsSync/* for synchronous form postback 
* */Views/Partials/FormEditor/FieldsAsync/* for asynchronous postback 

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

## Next step
Onwards to [Email templates](emails.md).
