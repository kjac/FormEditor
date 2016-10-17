angular.module("umbraco.directives").directive("formEditorMultipleEmails", [
  function () {
    var EMAIL_REGEXP = /^[_a-z0-9]+(\.[_a-z0-9]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,10})$/;
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