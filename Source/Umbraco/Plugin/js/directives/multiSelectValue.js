angular.module("umbraco.directives").directive("formEditorMultiSelectValue", [
  function () {
    return {
      restrict: "A",
      require: "ngModel",
      link: function (scope, element, attr, ngModel) {

        ngModel.$formatters.push(function (viewValue) {
          return validValue(viewValue);
        });

        function validValue(viewValue) {
          if (!viewValue) {
            return viewValue;
          }
          // #133 - multi value fields cannot have comma in their values
          return viewValue.replace(/,/g, "");
        }
      }
    }
  }
]);
