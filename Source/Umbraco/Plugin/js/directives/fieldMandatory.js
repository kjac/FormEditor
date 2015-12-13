angular.module("umbraco.directives").directive("formEditorFieldMandatory", [
  function () {
    return {
      restrict: "E",
      templateUrl: "formEditor.fieldMandatory.html"
    }
  }
]);
