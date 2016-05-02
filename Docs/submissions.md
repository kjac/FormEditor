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

## How about statistics?
If you'd like to work with the form submissions statistics on your frontend - you can! Once again, we start by retrieving the form model:

```cs
// get the form model (named "form" on the content type)
var form = Model.Content.GetPropertyValue<FormModel>("form");
```

The form model exposes the statistics for field value frequency through the method `GetFieldValueFrequencyStatistics()`. It can be used with the following parameters, all of which are optional:

- `IEnumerable<string> fieldNames`: The "form safe names" of the fields to retrieve statistics for. Default is all supported fields.
- `IPublishedContent content`: The content that holds the form. Default is current page.

Mind you, only a subset of the built-in Form Editor fields support field value frequency statistics, namely the ones that have a predefined value range (like radio button group, select box, etc).

### Sample template
Here's a full sample template that renders a bar chart (using [Google chart tools](https://developers.google.com/chart/)) of the field value frequencies for the field "genres":

```xml
@using FormEditor;
@inherits Umbraco.Web.Mvc.UmbracoTemplatePage
@{
  Layout = null;

  // get the form model (named "form" on the content type)
  var form = Model.Content.GetPropertyValue<FormModel>("form");
  
  // get the field value frequency statistics for the field "genres" (if it exists)  
  var statistics = form.GetFieldValueFrequencyStatistics(new[] { "genres" });
  var fieldValueFrequencies = statistics.FieldValueFrequencies.FirstOrDefault();
}
<!DOCTYPE html>
<html>
<head>
  <title>@Model.Content.Name</title>
  <link rel="stylesheet" href="http://getbootstrap.com/dist/css/bootstrap.min.css" />
  @if(fieldValueFrequencies != null)
  {
    
    @* read all about Google chart tools at https://developers.google.com/chart/ *@
    <script type="text/javascript" src="https://www.gstatic.com/charts/loader.js"></script>
    <script>
      google.charts.load("current", {packages: ["corechart", "bar"]});
      google.charts.setOnLoadCallback(drawChart);

      function drawChart() {

        @* this is the chart data. the first entry contains the chart legend - gonna leave that empty. *@
        var chartData = [
          ["", ""]
        ];

        @* add the field values and their frequencies *@
        @foreach(var fieldValueFrequency in fieldValueFrequencies.Frequencies)
        {
          @: chartData.push(["@fieldValueFrequency.Value", @fieldValueFrequency.Frequency]);
        }

        @* do the Google charts stuff *@
        var data = google.visualization.arrayToDataTable(chartData);
        var options = {
          chartArea: {width: "60%"},
          hAxis: {
            minValue: 0,
            title: "Total number of submissions: @statistics.TotalRows"
          },
          legend: "none"
        };
        var chart = new google.visualization.BarChart(document.getElementById("chart"));
        chart.draw(data, options);
      }
    </script>
  }
</head>
<body>
  @* this is where the chart will be rendered *@
  <div id="chart"></div>
</body>
</html>
```

The output should come out something like this:

![Frontend rendering of statistics](img/statistics.png)


## Wait... what about async?
Nope, sorry. There's no public endpoint for retrieving form submissions or statistics asynchronously. It would be a major security problem to have that.

## Next step
Onwards to [extending Form Editor](extend.md).