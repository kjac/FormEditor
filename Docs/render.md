# Rendering the form
Form Editor ships with two sample templates, one that demonstrates [synchronous form postback](../Source/Umbraco/Views/FormEditorSync.cshtml) and one that demonstrates [asynchronous form postback](../Source/Umbraco/Views/FormEditorAsync.cshtml) (using [AngularJS](https://angularjs.org/)). Once the package is installed, these sample templates are located at */Views/FormEditorSync.cshtml* and */Views/FormEditorAsync.cshtml* respectively. They are fairly well documented and will not be discussed in detail here. However, a few things are worth mentioning.

## Rendering rows, cells and fields
As mentioned in the [setup guide](install.md), the rows and cells have aliases to help you recognize them when rendering the form. 

As for field rendering, the sample templates use partial views to render all form fields. The partial views are referenced by convention based on the field `Type` name, and are expected to be located at:
* */Views/Partials/FormEditor/FieldsSync/* for synchronous form postback 
* */Views/Partials/FormEditor/FieldsAsync/* for asynchronous postback 

The following code sample shows how this could all be pieced together: 

```xml
@{
  // get the form model (named "form" on the content type)
  var form = Model.Content.GetPropertyValue<FormModel>("form");
}
<div class="container">
  @* NOTE: if you're not using form pages, you can access all form rows directly with form.Rows *@
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
                // render the form field with its partial view
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

## Submitting form data using synchronous postback
When using synchronous postback for form submission, simply call `CollectSubmittedValues()` on the `FormModel` property and everything (including redirects to the success page, if configured) will be handled for you: 

```cs
// get the form model (named "form" on the content type)
var form = Model.Content.GetPropertyValue<FormModel>("form");
// collect submitted values and redirect (this does nothing unless it's a postback)
form.CollectSubmittedValues();
```

## Submitting form data using asynchronous postback
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
