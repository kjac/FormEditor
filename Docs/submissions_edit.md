# Editing form submissions
If you need to edit form submissions, you can easily do so in frontend, at least if you're using the default partial views to render your form ([read more](render.md)). All you need to do is assign the ID of the form submission to `ViewBag.FormRowId` - it could be something like this: 

```cs
ViewBag.FormRowId = Request.QueryString["rowId"];
```

If the ID is valid, the partials will automatically load the currently submitted values for the form submission when displaying the form, and overwrite them when submitting the form.

## But... what ID?
Each row returned by `GetSubmittedValues()` ([read more](submissions_list.md)) contains the ID of the form submission in the `Id` property. A very crude listing of IDs could look like this:

```xml
@{
  var form = Model.Content.GetPropertyValue<FormModel>("form");
  var formData = form.GetSubmittedValues();
}
<ul>
  @foreach(var row in formData.Rows)
  {
    <li>
      <a href="?rowId=@row.Id">@row.Id</a>        
    </li>
  }
</ul>
```

## Next step
Onwards to [extending Form Editor](extend.md).