# Installing and setting up Form Editor
Form Editor works just like any other property editor for Umbraco, so installation is pretty straight forward. 

## Installing the package
First and foremost grab the [Form Editor package](https://github.com/kjac/FormEditor/releases/latest) from the latest release (it's the zip file attached to the release) and install it in the Developer section of your Umbraco site.

Or, if you're using NuGet, you can install the [Form Editor](https://www.nuget.org/packages/FormEditor/) NuGet package. 

## Setting up the data types
Once the package is installed, go ahead and create a Form Editor data type. 

![Form Editor data types](img/data-types.png)

### Configuring the Form Editor data type

##### Row layouts
In the "Row layouts" section of the configuration you'll set up the row layouts you want available for your editors. A row layout consists of:
* A **name** and an **icon**, so the editors can tell the row layouts apart.
* An **alias**, so you can identify the row layout when rendering the form.
* Some **cells** that the editors can add form fields to. A **cell** in turn consists of:
    * An **alias**, so you can identify the cell when rendering the form. 
    * The **width** of the cell (in percent of the total row width) when rendered in the Form Editor. Within a row layout, the sum of all cell widths must equal 100.

![Form Editor row layouts](img/row-layouts.png)

Form Editor ships with a bunch of row icons, but if you run out of icons you can add more simply by dumping them in */App_Plugins/FormEditor/editor/rows/*.

By default, Form Editor will suggest [Bootstrap](http://getbootstrap.com/css/#grid) style `.col-md-*` classes as cell aliases, mainly because the sample templates shipped with Form Editor use Bootstrap to render the form grid. But Form Editor is not tied to Bootstrap in any way. You have complete control over the [form rendering](render.md), so just use whatever cell alias that makes sense. 

##### Field type groups
Form Editor ships with a bunch of field types (textbox, email, select box etc.). By default they are all listed in alphabetical order when the editors add a new field to a form. You can change this by grouping the available field types into field type groups, which is a great way to help your editors find the field types they need. 

Don't want your editors adding certain field types to their forms? No problem. Just don't add these field types to any of the field type groups.

##### Email templates
Form Editor supports two different types of emails - notification emails (sent to specific recipients of the editor's choosing) and confirmation emails (sent to the end users when submitting the form).

You can choose separate email templates for notification and confirmation emails, or leave them blank if you don't want to support one or both types of emails. See [Email templates](emails.md) for more info.

Form Editor uses the mail settings configured in the `<mailSettings>` section of web.config for sending emails.

##### Tab order and availiability
There are a lot of options with Form Editor, some of which you might not use or want your editors to be concerned with. These options are grouped in tabs within the property editor. You can decide the order of these tabs as well as disable the tabs you don't want available to your editors.

##### Use submission approval
If your editors need to approve form submissions (e.g. for moderating comments), you can enable submission approval. This adds a little checkmark next to each submission, which the editors can click to approve the submission.

##### Web service integration
Form Editor can send form data automatically to an external web service upon a successful form submission. Read more about this [here](install_web_service.md).

##### Custom CSS
If you feel the need to style Form Editor differently, specify the path to your custom style sheet here and it will be loaded whenever a Form Editor property loads. The path must be from the root of your site.

##### The rest
Hopefully the rest of the Form Editor data type configuration is self explanatory. Oh, and it's highly recommended to tick the "Hide label" checkbox to give the Form Editor property as much space as possible.

## Setting up the content type
When you have configured the data type, create a content type (or reuse an existing) and add a property based on the new Form Editor data type. If you're going to use the templates and views shipped with Form Editor for rendering (see [Rendering the form](render.md)), make sure the Form Editor property has the property alias "form".

Since the Form Editor data type takes up a lot of space in the UI, you should consider placing the property on a separate tab - e.g. "Form".

**Please note:** You can only have *one* Form Editor property per content type.

**Please also note:** Due to backwards compatibility issues, you *cannot* put the Form Editor property in a content type composition. At least not for the time being.

## Next step
Onwards to [Rendering the form](render.md).
