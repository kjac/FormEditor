angular.module("umbraco.directives").directive("formEditorMultipleEmails", [
  function () {
    var EMAIL_REGEXP = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return {
      require: "ngModel",
      restrict: "A",
      link: function (scope, element, attrs, ctrl) {
        ctrl.$parsers.unshift(function (viewValue) {
          if (!viewValue) {
            ctrl.$setValidity("formEditorMultipleEmails", true);
            return viewValue;
          }

          var valid = _.every(viewValue.split(','), function (email) {
            return EMAIL_REGEXP.test(email.trim());
          });

          if (valid) {
            ctrl.$setValidity("formEditorMultipleEmails", true);
            return viewValue;
          } else {
            ctrl.$setValidity("formEditorMultipleEmails", false);
            return undefined;
          }
        });
      }
    };
  }
]);