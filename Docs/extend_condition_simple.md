# Creating a simple custom condition
By completing this steps you can create a simple custom condition:

1. Register the condition in the `<CustomConditions/>` section of [*/Config/FormEditor.config*](../Source/Umbraco/Config/FormEditor.config).
2. Create an icon as a 16x16 px PNG (preferably grayscale) and save it to */App_Plugins/FormEditor/editor/conditions/*.
3. Implement JS for the frontend.

The following shows how to implement a custom condition that tests if the length of a field value is less than 10 characters.

## Step 1: Register the condition
Open */Config/FormEditor.config* and add the condition to the <CustomConditions/> section:

```xml
<FormEditor>
  <CustomConditions>
    <Condition type="my.length" name="Less than 10 characters" />
  </CustomConditions>
  <!-- ... -->
</FormEditor>
```

* The `type` of the condition must be unique across all conditions, as it's used as an identifier throughout the system. 
* The `name` is the condition name shown to your editors when they're working with validations or actions. 

**Note**: You may have to restart the site to make Form Editor pick up the new configuration.

## Step 2: Create the condition icon
If you want an icon that looks like the ones used by Form Editor, pick one from  the Fugue Icons set at http://p.yusukekamiyamane.com/. Or use this one:

![Number icon](img/my.length.png)

The condition icon must be named like the `type` name of the condition. In this case it's `my.length`, so the icon needs to be saved as */App_Plugins/FormEditor/editor/conditions/my.length.png*.

## Implement JS for the frontend
You can plug the custom condition JS into the Form Editor JS using the globally available entry point `addFormEditorCondition` (read all about that [here](extend_condition_advanced.md#implement-js-for-the-frontend)). The JS required is a bit different depending on your choice of synchronous or asynchronous form postback.

### For synchronous form postback
```js
addFormEditorCondition("my.length", function (rule, fieldValue, $form) {
  // the condition is fulfilled if the field value is null or the field value length is less than 10 characters
  return (fieldValue == null || fieldValue.length < 10);
});
```

### For asynchronous form postback
```js
addFormEditorCondition("my.length", function (rule, fieldValue, formData) {
  // the condition is fulfilled if the field value is null or the field value length is less than 10
  return (fieldValue == null || fieldValue.length < 10);
});
```

