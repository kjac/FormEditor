angular.module("umbraco.directives").directive("formEditorOnEnter", [
  function (formEditorLocalizationService) {
    var linker = function (scope, element, attrs) {
      element.bind("keypress", function (event) {
        if (event.which === 13) {
          scope.$apply(function () {
            scope.$eval(attrs.formEditorOnEnter);
          });
          event.preventDefault();
        }
      });
    };

    return {
      restrict: "A",
      link: linker
    }
  }
]);
