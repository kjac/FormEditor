# Multiple forms per page
Although you can only have one Form Editor property on an Umbraco document type, you can still render multiple forms per page using the technique described in [reusable forms](reuse.md). The premade partial views for  [synchronous](../Source/Umbraco/Views/Partials/FormEditor/Sync.cshtml) and [asynchronous](../Source/Umbraco/Views/Partials/FormEditor/Async.cshtml) form postback both support this scenario out of the box. 

But... If you're using asynchronous form postback, you'll need to add a small tweak to make AngularJS play nice.

## Multiple forms and AngularJS
The partial view for asynchronous form postback declares an `ng-app` each time it's rendered. This becomes a problem when rendering multiple forms on the same page, because AngularJS does not allow for more than one `ng-app`.

To overcome this you'll need to do three things:

1. Tell the partial view not to declare `ng-app`.
2. Create your own `ng-app` which takes a dependency on the *formEditor* angular module.
3. Declare your own `ng-app` in a scope that contains all the partial views (e.g. on the `<body>` tag).

The first step is easy; just add `ViewBag.FormAppDeclared = true;` before rendering the partial view: 

```cs
// tell the Form Editor async rendering that ng-app is declared in an outer scope
ViewBag.FormAppDeclared = true;
// call the Form Editor async rendering
@Html.Partial("FormEditor/Async", Umbraco.AssignedContentItem)
```

The second step isn't much harder. Declare a new `ng-app` (or add the *formEditor* module to your existing app):

```html
<script type="text/javascript">
  // this is your site app. it needs to include the "formEditor" module.
  var myApp = angular.module("myApp", ["formEditor" /* add more modules to your app here */])
</script>
```

And lastly, declare your `ng-app` in the outer scope:
```html
<body ng-app="myApp">
```

## Next step
Onwards to [working with form submissions](submissions.md).