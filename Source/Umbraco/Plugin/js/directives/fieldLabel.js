angular.module("umbraco.directives").directive("formEditorFieldLabel", [
  function () {
    return {
      restrict: "E",
      templateUrl: "formEditor.fieldLabel.html"
    }
  }
]);
