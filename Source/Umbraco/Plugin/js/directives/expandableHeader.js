angular.module("umbraco.directives").directive("formEditorExpandableHeader", [
  function () {
    return {
      scope: {
        expandable: "="
      },
      restrict: "E",
      templateUrl: "formEditor.expandableHeader.html",
      link: function (scope, element, attributes) {
        scope.headerTextKey = attributes["headerTextKey"] || "missing.headerTextKey";
        scope.headerTextDefault = attributes["headerTextDefault"] || "Missing headerTextDefault";
      }
    }
  }
]);
