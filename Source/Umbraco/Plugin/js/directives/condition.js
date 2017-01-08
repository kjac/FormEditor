angular.module("umbraco.directives").directive("formEditorCondition", ["$http", "$compile", "formEditorPropertyEditorResource",
  function ($http, $compile, formEditorPropertyEditorResource) {
    var linker = function (scope, element, attrs) {
      attrs.$observe("type", function () {
        $http.get(formEditorPropertyEditorResource.pathToConditionFile(scope.rule.condition.view + ".html")).then(function (result) {
          element.html(result.data);
          $compile(element.contents())(scope);
        });
      });
    }

    return {
      restrict: "E",
      link: linker
    }
  }
]);
