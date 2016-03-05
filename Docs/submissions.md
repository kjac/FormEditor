# Working with the form submissions
Most of the time you'll probably just work with the form submissions in the Umbraco back office. After all, that's pretty much the whole point - right?

But... who's to say you don't want to access the same data from the frontend? It's pretty straight forward with Form Editor.

## Talk is cheap - show me the code!
You'll want to start out by retrieving your form model from the content that contains it.

```cs
// get the form model (named "form" on the content type)
var form = Model.Content.GetPropertyValue<FormModel>("form");
```

The form model exposes its form submissions through the method `GetSubmittedValues()`. It can be used with a bunch of different parameters, all of which are optional:

- `int page`: The page number to retrieve. 1-based, default is 1.
- `int perPage`: The page size. Default is 10.
- `string sortField`: The "form safe name" of the field to use for sorting. Default is the date of submission.
- `bool sortDescending`: Determines the sort order. Default is `false`.
- `IPublishedContent content`: The content that holds the form. Default is current page.

`GetSubmittedValues()` returns a [`FormData`](../Source/Solution/FormEditor/Data/FormData.cs) object, which contains the form submissions.  

```cs
var formData = form.GetSubmittedValues(
  page: 1, 
  perPage: 10, 
  sortField: "myField", 
  sortDescending: true, 
  content: Model.Content
);
```

## Putting it all together
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

## Wait... what about async?
Nope, sorry. There's no public endpoint for retrieving form submissions asynchronously. It would be a major security problem to have that.

## Next step
Onwards to [extending Form Editor](extend.md).