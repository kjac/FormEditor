angular.module("formEditor", [])
  .controller("FormController", ["$scope", "$filter", "$http", "$window", function ($scope, $filter, $http, $window) {
    $scope.formData = {};
    $scope.fileData = {};

    // fetch the validations from global variable
    $scope.validations = _formValidations;
    $scope.invalidValidations = [];

    $scope.submitStatus = "none";
    $scope.showReceipt = false;

    $scope.activePage = 0;

    // initialize field default values from global variable
    for (var key in _formDefaultValues) {
      $scope.formData[key] = _formDefaultValues[key];
    }

    $scope.toggleMultiSelectValue = function (fieldName, value, required) {
      var values = $scope.formData[fieldName] || [];
      var index = values.indexOf(value);
      if (index < 0) {
        values.push(value);
      } else {
        values.splice(index, 1);
      }
      if (values.length == 0) {
        values = undefined;
      }
      $scope.formData[fieldName] = values;
      if (required) {
        $scope.form[fieldName].$setValidity("required", values != undefined && values.length != 0);
      }
    };

    $scope.hasMultiSelectValue = function (fieldName, value) {
      var values = $scope.formData[fieldName] || [];
      return values.indexOf(value) >= 0;
    };

    $scope.submit = function () {
      if ($scope.form.$invalid) {
        return;
      }

      $scope.invalidValidations = $filter("filter")($scope.validations, function (validation, index, array) {
        return $scope.validate(validation) == false;
      });
      if ($scope.invalidValidations.length > 0) {
        return;
      }

      var data = new FormData();
      // add form ID from global variable
      data.append("_id", _formId);
      // add form data
      for (var key in $scope.formData) {
        var value = $scope.formData[key];
        if (value == null || value == undefined) {
          continue;
        }
        if (value instanceof Date) {
          data.append(key, value.toISOString());
        }
        else {
          data.append(key, value);
        }
      }
      // add file data
      for (var key in $scope.fileData) {
        data.append(key, $scope.fileData[key]);
      }
      // special case: add reCAPTCHA response if present
      // - fetch from document.form because it's not an angular model
      if ($window.document.form["g-recaptcha-response"]) {
        data.append("g-recaptcha-response", $window.document.form["g-recaptcha-response"].value);
      }

      $scope.submitStatus = "submitting";

      // post the form data to the public SubmitEntry endpoint
      $http
        .post("/umbraco/FormEditorApi/Public/SubmitEntry/", data, { headers: { "Content-Type": undefined } })
        .then(function successCallback(response) {
          $scope.submitStatus = "success";
          if (response.data && response.data.redirectUrl) {
            $window.location.href = response.data.redirectUrl;
          }
          $scope.showReceipt = true;
          // add your own success handling here
        }, function errorCallback(response) {
          $scope.submitStatus = "failure";
          if (response.data) {
            if (response.data.invalidFields && response.data.invalidFields.length > 0) {
              angular.forEach(response.data.invalidFields, function (field) {
                $scope.form[field.formSafeName].$invalid = true;
              });
            }
            if (response.data.failedValidations && response.data.failedValidations.length > 0) {
              $scope.invalidValidations = response.data.failedValidations;
            }
          }
          // add your own error handling here
        });
    };

    // validate a validation (usually cross field validation)
    $scope.validate = function (validation) {
      if (!validation.rules || !validation.rules.length) {
        // edge case: validation contains no rules. must be valid.
        return true;
      }
      var isValid = false;
      angular.forEach(validation.rules, function (rule) {
        // get the field value from form data
        var fieldValue = $scope.formData[rule.field.formSafeName];
        // treat empty and undefined values as null values for a cleaner validation below
        if (fieldValue == undefined || fieldValue == "") {
          fieldValue = null;
        }
        // concat array values (e.g. select boxes and checkbox groups) to comma separated strings
        if (angular.isArray(fieldValue)) {
          fieldValue = fieldValue.join();
        }

        // now check if the field value matches the rule condition
        var ruleIsFulfilled = false;
        var conditionCallback = getFormEditorCondition(rule.condition.type);
        if (conditionCallback) {
          try {
            ruleIsFulfilled = conditionCallback(rule, fieldValue, $scope.formData);
          }
          catch (err) {
            // log error and continue (and hope the server side validation handles things)
            console.log(err);
          }
        }

        // all rules must be fulfilled for a validation to fail
        if (ruleIsFulfilled == false) {
          // no need to continue the loop
          isValid = true;
          return false;
        }
      });
      return isValid;
    };

    $scope.$on("filesSelected", function (event, args) {
      if (args.files.length == 0) {
        $scope.fileData[args.fieldName] = undefined;
      } else {
        // currently no support for multiple files
        $scope.fileData[args.fieldName] = args.files[0];
      }
    });

    // form paging
    $scope.isFirstPage = function() {
      return $scope.activePage == 0;
    };
    $scope.isLastPage = function () {
      return $scope.activePage == (_formTotalPages - 1);
    };
    $scope.isActivePage = function (pageNumber) {
      return $scope.activePage == pageNumber;
    };
    $scope.goToNextPage = function() {
      if ($scope.isLastPage() == false) {
        $scope.activePage++;
      }
    }
    $scope.goToPreviousPage = function () {
      if ($scope.isFirstPage() == false) {
        $scope.activePage--;
      }
    }
  }])
  .directive("fileUpload", function () {
    return {
      restrict: "A",
      scope: true,
      link: function (scope, el, attrs) {
        el.bind('change', function (event) {
          scope.$emit("filesSelected", { fieldName: event.target.name, files: event.target.files });
        });
      }
    };
  })
  .directive("httpPrefix", function () {
    return {
      restrict: "A",
      require: "ngModel",
      link: function (scope, element, attrs, controller) {
        function ensureHttpPrefix(value) {
          // Need to add prefix if we don't have http:// prefix already AND we don't have part of it
          if (value && !/^(https?):\/\//i.test(value)
             && "http://".indexOf(value) === -1) {
            controller.$setViewValue("http://" + value);
            controller.$render();
            return "http://" + value;
          }
          else
            return value;
        }
        controller.$formatters.push(ensureHttpPrefix);
        controller.$parsers.splice(0, 0, ensureHttpPrefix);
      }
    };
  });


// global container and functions for handling cross field validation conditions
// - makes it easier to extend the validation conditions
var formEditorConditions = {};
function addFormEditorCondition(type, callback) {
  formEditorConditions[type] = callback;
}
function getFormEditorCondition(type) {
  return formEditorConditions[type];
}

// add core validation conditions
// - "field is not empty" condition:
addFormEditorCondition("core.fieldisnotempty", function (rule, fieldValue, formData) {
  return (fieldValue != null);
});
// - "field is empty" condition (negation of "field is not empty" condition):
addFormEditorCondition("core.fieldisempty", function (rule, fieldValue, formData) {
  return !(getFormEditorCondition("core.fieldisnotempty")(rule, fieldValue, formData));
});
// - "field value is not" condition:
addFormEditorCondition("core.fieldvalueisnot", function (rule, fieldValue, formData) {
  return (fieldValue || "").toLowerCase() != (rule.condition.expectedFieldValue || "").toLowerCase();
});
// - "field value is" condition (negation of "field value is not" condition):
addFormEditorCondition("", function (rule, fieldValue, formData) {
  return !(getFormEditorCondition("core.fieldvalueisnot")(rule, fieldValue, formData));
});
// - "field value does not other field value" condition:
addFormEditorCondition("core.fieldvaluesdonotmatch", function (rule, fieldValue, formData) {
  var otherFieldValue = formData[rule.condition.otherFieldName];
  return (fieldValue || "").toLowerCase() != (otherFieldValue || "").toLowerCase();
});
// - "field value matches other field value" condition (negation of "field value does not other field value" condition):
addFormEditorCondition("core.fieldvaluesmatch", function (rule, fieldValue, formData) {
  return !(getFormEditorCondition("core.fieldvaluesdonotmatch")(rule, fieldValue, formData));
});
