angular.module("umbraco.directives").directive("formEditorLocalize", ["formEditorLocalizationService",
  function (formEditorLocalizationService) {
    var linker = function (scope, element, attrs) {

      attrs.$observe("key", function () {
        var key = attrs.key;
        var defaultValue = attrs.defaultValue;
        formEditorLocalizationService.localize(key, defaultValue).then(function (value) {
          if (value) {
            element.html(value);
          }
        });
      });
    }

    return {
      restrict: "EA",
      replace: true,
      link: linker
    }
  }
]);
