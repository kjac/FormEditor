# Form Editor for Umbraco

Form Editor is a free and open source form builder for [Umbraco](http://umbraco.com/) 7.3+ that lets your editors build forms and manage form submissions as just another part of the Umbraco content. It might also just be the easiest form builder you've ever had to integrate with your site.

![Form Editor UI](Docs/img/form layout.png)

If you're new to Form Editor, you should probably start by checking out the [Quick start tutorial](Tutorials/QuickStart.md).

Some highlights include:
* Grid based form editing.
* Ships with (almost) all HTML5 input types.
* Cross field validation.
* Support for multi-page forms (optional).
* Full control over the frontend rendering.
* Full support for asynchronous postback, e.g. for AngularJS.
* Easily extendable with custom fields.
* Built-in statistics for form submissions.
* Editors can add texts and images alongside the form fields.
* reCAPTCHA support.
* Fully localizable.

## Table of contents
* [Installing and setting up Form Editor](Docs/install.md)
* [Rendering the form](Docs/render.md)
* [Email templates](Docs/emails.md)
* [Special form fields](Docs/fields.md)
* [Reusable forms](Docs/reuse.md)
* [Multiple forms per page](Docs/multiple.md)
* [Working with the form submissions](Docs/submissions.md)
* [Extending Form Editor (custom fields, workflows and more)](Docs/extend.md)
* [Creating a default form](Docs/initialize.md)
* [A note about storage](Docs/storage.md)
* [Building and contributing](Docs/build.md)

## Tutorials
* [Quick start tutorial](Tutorials/QuickStart.md) - start here if you're new to Form Editor.
    * Topics covered: Getting started.
* [Hello Form Editor](Tutorials/HelloFormEditor.md) - add Form Editor to the Fanoe starter kit in just 15 minutes.
    * Topics covered: Getting started, form reuse.
* [Creating a poll with Form Editor](Tutorials/Poll.md) - use Form Editor to create polls within the Fanoe starter kit.
    * Topics covered: Forms within the grid, form submission statistics, asynchronous form postback, multiple forms per page.
* [User ratings with Form Editor](Tutorials/Ratings.md) - build an article rating system with Form Editor.
    * Topics covered: Form submission rendering and statistics, customized field rendering.
    * [User ratings - part two: Custom rating field](Tutorials/RatingsPartTwo.md) - create a custom field for star ratings.
        * Topics covered: Custom field creation both by configuration and by code.
    * [User ratings - part three: Automatically creating the form](Tutorials/RatingsPartThree.md) - create the rating form automatically.
        * Topics covered: Creating default forms, customizing the property editor.
    * [User ratings - part four: Listing the articles](Tutorials/RatingsPartFour.md) - extract ratings for a list view.
        * Topics covered: Form Editor submission events, custom data layer for statistics data.

## Credits
A huge thank-you goes out to the talented [Yusuke Kamiyamane](http://p.yusukekamiyamane.com/) for creating the Fugue Icons that are used heavily in this project. H5YR!
