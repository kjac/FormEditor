# Listing form submissions
If you want to list the submissions made to a form, it's pretty straight forward with Form Editor.

You'll want to start out by retrieving your form model from the content that contains it.

```cs
// get the form model (named "form" on the content type)
var form = Model.Content.GetPropertyValue<FormModel>("form");
```

The form model exposes its form submissions through the method `GetSubmittedValues()`. It can be used with a bunch of different parameters, all of which are optional:

- `IPublishedContent content`: The content that holds the form. Default is current page.
- `int page`: The page number to retrieve. 1-based, default is 1.
- `int perPage`: The page size. Default is 10.
- `string sortField`: The "form safe name" of the field to use for sorting. Default is the date of submission.
- `bool sortDescending`: Determines the sort order. Only applicable if `sortField` is specified. Default is `false`.
- `ApprovalState approvalState`: The approval state of the submissions. Only applicable if "Use submission approval" is enabled on the Form Editor datatype. Default is `ApprovalState.Approved`.

`GetSubmittedValues()` returns a [`FormData`](../Source/Solution/FormEditor/Data/FormData.cs) object, which contains the form submissions.  

```cs
var formData = form.GetSubmittedValues(
  content: Model.Content,
  page: 1, 
  perPage: 10, 
  sortField: "myField", 
  sortDescending: true, 
  approvalState: ApprovalState.Any
);
```

### Sample template
Here's a full sample template that outputs the most recent form submissions to the users:

```xml
@using FormEditor;
@inherits Umbraco.Web.Mvc.UmbracoTemplatePage
@{
  Layout = null;

  // get the form model (named "form" on the content type)
  var form = Model.Content.GetPropertyValue<FormModel>("form");

  // fetch the 10 most recent form submissions
  var formData = form.GetSubmittedValues();
}
<!DOCTYPE html>
<html>
<head>
  <title>@Model.Content.Name</title>
  <link rel="stylesheet" href="http://getbootstrap.com/dist/css/bootstrap.min.css" />
</head>
<body>
  @if(formData.Rows.Any())
  {
    <h2>Here are the @formData.Rows.Count() most recent submissions (from a total of @formData.TotalRows)</h2>
    <table class="table table-striped">
      @* create the table header using the field names *@
      <thead>
        <tr>
          @foreach(var field in formData.Fields)
          {
            <th>@field.Name</th>
          }
        </tr>
      </thead>
      @* create the table body by iterating all rows *@
      <tbody>
        @foreach(var row in formData.Rows)
        {
          <tr>
            @foreach(var field in row.Fields)
            {
              <td>@field.Value</td>
            }
          </tr>
        }
      </tbody>
    </table>
  }
</body>
</html>
```

## Next step
Onwards to [working with form submission statistics](submissions_stats.md) or read about [extending Form Editor](extend.md).