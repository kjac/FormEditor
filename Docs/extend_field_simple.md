# Creating a simple custom field
A simple custom field is created by completing these steps:

1. [Create your icon](extend_field.md) and save it to */App_Plugins/FormEditor/editor/fields/*.
2. Register your field in the `<CustomFields/>` section of [*/Config/FormEditor.config*](../Source/Umbraco/Config/FormEditor.config).
3. Create the partial view to render your field.

The following exercise runs through the steps above to create a custom slider field (`input type="range"`). The complete solution can be found under [Samples](../Samples/Simple custom field/).

## Step 1: Create the field icon
Go ahead and pick one of the *slider* icons from the Fugue Icons set over at http://p.yusukekamiyamane.com/. 

Form Editor matches everything up by the `type` name of the fields. This needs to be unique across all fields. In this exercise you'll use `my.range` as field type name, and thus your field icon should be saved as */App_Plugins/FormEditor/editor/fields/my.range.png*.

## Step 2: Register the field
Open */Config/FormEditor.config* and add your field to the <CustomFields/> section:
```xml
<FormEditor>
  <CustomFields>
    <Field type="my.range" name="Slider" />
  </CustomFields>
  <!-- ... -->
</FormEditor>
```

The `name` given here is the field name presented to your editors when they're layouting the form. In case you want to localize the field name you can add an entry called `composition.field.my.range` to the localization files under */App_Plugins/FormEditor/js/langs/*.

**Note**: You may have to restart the site to make Form Editor pick up the new configuration.

### Help! I can't find my field!?
If your custom field doesn't show up in the Form Editor even after restarting the site, maybe you forgot to add it to a field type group? Read more about field type groups [here](install.md).

## Step 3: Render the field
The last step is to create the partial view that will render your slider field for the end users. Again, the partial view must be named after the field type name, so in this case it will be `my.range.cshtml`.

Create the partial view in the folder where your form template looks for field partials. See the [rendering section](render.md) for details.

The following example illustrates of how the partial view could be implemented (of course the actual implementation is entirely up to you):

```xml
@inherits Umbraco.Web.Mvc.UmbracoViewPage<FormEditor.Fields.CustomField>
<div class="form-group @(Model.Invalid ? "has-error" : null)">
  <label for="@Model.FormSafeName">@Model.Label</label>
  <input type="range" id="@Model.FormSafeName" name="@Model.FormSafeName" value="@Model.SubmittedValue" min="0" max="100" step="10" @(Model.Mandatory ? "required" : null) />

  @Html.Partial("FormEditor/FieldsSync/core.utils.helptext")
  @Html.Partial("FormEditor/FieldsSync/core.utils.validationerror")
</div>
```

Notice the hard-coded values for `min`, `max` and `step` in the input field. If you had implemented the field [the advanced way](extend_field_advanced.md), you could have made these values configurable by the editors.

### Points of interest
A few things are worth pointing out in the field partial above:
* `@Model.FormSafeName` is the name your field must use within the form, so Form Editor can pick up the submitted value upon form submission.
* `@Model.SubmittedValue` is the value submitted to the field by the end user. It will be empty unless in the following scenarios:
    1. The form was submitted, but was found to be invalid by the server side validation, and thus the form was re-rendered.
    2. The form was submitted, but no success page was configured for redirection upon successful form submission, and thus the form was re-rendered.
* The two partial views `core.utils.helptext` and `core.utils.validationerror` are generic helpers for rendering the field help text and validation error (if the field value is invalid). Use them or don't, that's entirely up to you.

## Fields with fixed values
If you want the editors to be able to enter a range of fixed values when configuring the your custom field (like a select box), set `fixedValues` to `true` in */Config/FormEditor.config*:

```xml
<FormEditor>
  <CustomFields>
    <Field type="my.options" name="Options" fixedValues="true" />
  </CustomFields>
  <!-- ... -->
</FormEditor>
```

Form Editor will then add the fixed field values configuration to the field configuration window in Umbraco. 

When you render your field, you need to use `FormEditor.Fields.CustomFieldFixedValues` instead of `FormEditor.Fields.CustomField`. The fixed field values will be available in `Model.FieldValues`:

```xml
@inherits Umbraco.Web.Mvc.UmbracoViewPage<FormEditor.Fields.CustomFieldFixedValues>
<div class="form-group @(Model.Mandatory ? "required" : null) @(Model.Invalid ? "has-error" : null)">
  <label for="@Model.FormSafeName">@Model.Label</label>
  <select class="form-control" id="@Model.FormSafeName" name="@Model.FormSafeName" @(Model.Mandatory ? "required" : null)>
    @foreach (var fieldValue in Model.FieldValues)
    {
      <option value="@fieldValue.Value" @(fieldValue.Selected ? "selected" : "")>@fieldValue.Value</option>
    }
  </select>

  @Html.Partial("FormEditor/FieldsSync/core.utils.helptext")
  @Html.Partial("FormEditor/FieldsSync/core.utils.validationerror")
</div>
```

Any values submitted by the end users to this field will be validated server side against the configured range of values. And as an added bonus, the field will automatically support field value statistics.
