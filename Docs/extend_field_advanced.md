# Creating an advanced custom field
An advanced custom field is created by completing these steps:
1. [Create your icon](extend.md).
2. Implement your field.
3. Create the editor to configure your field.
4. Create the partial view to render your field.

Unlike when creating a [simple custom field](extend_field_simple.md) there's no need to register your advanced custom field anywhere. Simply copy your DLL and the rest of your field assets to your site, and Form Editor will automatically detect your field when the site restarts.

A complete implementation of a custom slider field (`input type="range"`) can be found under [Samples](../Samples/). The sample is meant to serve as documentation on its own, so the details of the implementation will not be discussed here. A few things are worth digging into, though.

## Project structure and field assets
The field assets (icon, editor and view) are included in the sample project. The directory structure in which they are placed mirrors the structure of your site (at least if you're using the [sample templates](../Source/Umbraco/Views/)). In other words you can copy the *App_Plugins* and *Views* folders directly to your site, and the assets will be placed correctly:
* The icon and editor goes into */App_Plugins/FormEditor/editor/fields/*.
* The partial view(s) goes into the folder where your form template looks for field partials. See the [rendering section](rendering.md) for details.

## Project output
The sample project references the [Umbraco Cms Core Binaries](https://www.nuget.org/packages/UmbracoCms.Core/) NuGet package. That's why a bunch of Umbraco DLLs are found in the project output bin folder. Don't copy all of these to your site, or it will most likely explode. Please copy only the project output DLL (`My.Range.dll`).

## Rendering the field
The partial views in the sample project are almost identical with the ones in the simple custom field sample, with the exception of using the configured field properties rather than hard-coded values for `min`, `max` and `step` in the input field.

See the [simple custom field section](extend_field_simple.md) for more details.
