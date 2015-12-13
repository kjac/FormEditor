angular.module("umbraco.directives").directive("formEditorFieldErrorMessage", [
  function () {
    return {
      restrict: "E",
      templateUrl: "formEditor.fieldErrorMessage.html"
    }
  }
]);
