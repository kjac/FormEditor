angular.module("umbraco.directives").directive("formEditorFieldOptions", [
  function () {
    return {
      restrict: "E",
      templateUrl: "formEditor.fieldOptions.html",
      link: function (scope, element, attributes) {
        scope.optionSelectedTextKey = attributes["optionSelectedTextKey"] || "edit.options.selected";
        scope.optionSelectedTextDefault = attributes["optionSelectedTextDefault"] || "Selected";
        scope.multiValueField = attributes["multiValueField"] ? true : false;
      }
    }
  }
]);
