angular.module("formEditor", [])
  .controller("FormController", ["$scope", "$filter", "$http", "$window", "$timeout", "$q", function ($scope, $filter, $http, $window, $timeout, $q) {
    $scope.formData = {};
    $scope.fileData = {};

    $scope.formState = {};
    $scope.invalidValidations = [];
    $scope.invalidFields = [];

    $scope.submitStatus = "none";
    $scope.showReceipt = false;

    $scope.activePage = 0;

    $scope.init = function (formId) {
      if (typeof _fe == "undefined" || typeof _fe[formId] == "undefined") {
        console.error("Could not find any Form Editor state for form ID " + formId);
        return;
      }

      $scope.formState = _fe[formId];
      $scope.formState.formId = formId;
      for (var key in $scope.formState.defaultValues) {
        $scope.formData[key] = $scope.formState.defaultValues[key];
      }

      if ($scope.formState.actions && $scope.formState.actions.length) {
        $scope.fieldVisibility = {};
        $scope.$watch("formData", function (newValue, oldValue, scope) {
          $scope.formDataChanged();
        }, true);
      }

      // create a global scope access to form validation and submission
      $window.feGlobal = $window.feGlobal || [];
      $window.feGlobal[formId] = {
        submit: $scope.globalSubmitForm,
        validate: $scope.globalValidateForm,
        setValue: $scope.globalSetFieldValue
      };
    }

    $scope.toggleMultiSelectValue = function (fieldName, pageNumber, value, required) {
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
        $scope.getFormPage(pageNumber)[fieldName].$setValidity("required", values != undefined && values.length != 0);
      }
    };

    $scope.hasMultiSelectValue = function (fieldName, value) {
      var values = $scope.formData[fieldName] || [];
      return values.indexOf(value) >= 0;
    };

    // this exposes the form validation to a global scope
    $scope.globalValidateForm = function () {
      var deferred = $q.defer();
      $timeout(
        function () {
          var valid = $scope.validateOnSubmit();
          deferred.resolve(valid);
        },
        50
      );
      return deferred.promise;
    }

    // this exposes the form submission to a global scope
    $scope.globalSubmitForm = function () {
      var deferred = $q.defer();
      $timeout(
        function () {
          deferred.resolve($scope.submit());
        },
        50
      );
      return deferred.promise;
    }

    // this exposes a way of setting a field value from the global scope
    $scope.globalSetFieldValue = function(fieldName, value) {
      $timeout(
        function () {
          $scope.formData[fieldName] = value;
        },
        50
      );
    }

    $scope.validateOnSubmit = function () {
      $scope.invalidFields = [];
      for (var i = 0; i < $scope.formState.totalPages; i++) {
        $scope.getFormPage(i).showValidationErrors = true;
      }

      if ($scope.form.$invalid) {
        return false;
      }

      $scope.invalidValidations = $filter("filter")($scope.formState.validations, function (validation, index, array) {
        return $scope.validate(validation) == false;
      });
      if ($scope.invalidValidations.length > 0) {
        return false;
      }

      return true;
    }

    $scope.submit = function () {
      var deferred = $q.defer();

      var valid = $scope.validateOnSubmit();
      if (valid == false) {
        deferred.resolve(false, null);
        return;
      }

      var data = new FormData();
      // add form ID from global variable
      data.append("_id", $scope.formState.formId);
      data.append("_rowId", $scope.formState.formRowId);
      // add form data
      for (var key in $scope.formData) {
        var value = $scope.formData[key];
        if (value == null || value == undefined) {
          continue;
        }
        if (value instanceof Date) {
          data.append(key, value.toISOString());
        }
        else if (value instanceof Object) {
          data.append(key, JSON.stringify(value));
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
      var formElement = $window.document.getElementById("fe_" + $scope.formState.formId);
      if (formElement && formElement["g-recaptcha-response"]) {
        data.append("g-recaptcha-response", formElement["g-recaptcha-response"].value);
      }

      $scope.submitStatus = "submitting";

      // post the form data to the public SubmitEntry endpoint
      $http
        .post("/umbraco/FormEditorApi/Public/SubmitEntry/", data, { headers: { "Content-Type": undefined, "AntiForgeryToken": $scope.formState.antiForgeryToken } })
        .then(function successCallback(response) {
          $scope.submitStatus = "success";
          if (response.data && response.data.redirectUrl) {
            $window.location.href = response.data.redirectUrl;
          }
          $scope.showReceipt = $scope.formState.hasReceipt;
          // add your own success handling here
          deferred.resolve(true, response.data);
        }, function errorCallback(response) {
          $scope.submitStatus = "failure";
          if (response.data) {
            if (response.data.invalidFields && response.data.invalidFields.length > 0) {
              angular.forEach(response.data.invalidFields, function (f) {
                var field = $scope.getFormField(f.formSafeName);
                if (field != null) {
                  field.$setValidity("required", false);
                }
              });
            }
            if (response.data.invalidFields && response.data.invalidFields.length > 0) {
              $scope.invalidFields = response.data.invalidFields;
            }
            if (response.data.failedValidations && response.data.failedValidations.length > 0) {
              $scope.invalidValidations = response.data.failedValidations;
            }
          }
          // add your own error handling here
          deferred.resolve(false, response.data);
        });

      return deferred.promise;
    };

    // validate a validation (usually cross field validation)
    $scope.validate = function (validation) {
      if (!validation.rules || !validation.rules.length) {
        // edge case: validation contains no rules. must be valid.
        return true;
      }

      // a validation fails if all rules are fulfilled
      return $scope.validateRules(validation.rules) == false;
    };

    // validate a set of rules
    $scope.validateRules = function (rules) {
      var allRulesFulfilled = true;
      angular.forEach(rules, function (rule) {
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
            console.warn(err);
          }
        }

        // all rules must be fulfilled for a validation to fail
        if (ruleIsFulfilled == false) {
          allRulesFulfilled = false;
          // no need to continue the loop
          return false;
        }
      });
      return allRulesFulfilled;
    };

    $scope.$on("filesSelected", function (event, args) {
      if (args.files.length == 0) {
        $scope.fileData[args.fieldName] = undefined;
      } else {
        // currently no support for multiple files
        $scope.fileData[args.fieldName] = args.files[0];
        var field = $scope.getFormField(args.fieldName);
        if (field != null) {
          // #163 - make sure we reset any validation errors set by server side validation
          field.$setValidity("required", true);
        }
      }
    });

    $scope.shouldShowValidationError = function (fieldName, pageNumber) {
      var formPage = $scope.getFormPage(pageNumber);
      var field = formPage[fieldName];
      if (field == null) {
        return formPage.showValidationErrors && $scope.invalidFields.length && $filter('filter')($scope.invalidFields, { formSafeName: fieldName }).length;
      }
      if (field.$invalid == false) {
        return false;
      }
      return formPage.$invalid && formPage.showValidationErrors;
    }

    $scope.formDataChanged = function() {
      angular.forEach($scope.formState.actions, function (action) {
        var allRulesFulfilled = $scope.validateRules(action.rules);
        switch (action.task) {
          case "core.showfield":
          case "core.hidefield":
            var show = action.task == "core.showfield";
            $scope.fieldVisibility[action.field.formSafeName] = (show == allRulesFulfilled);
            break;
        }
      });
    }

    $scope.getFormField = function (formSafeName) {
      for (var i = 0; i < $scope.formState.totalPages; i++) {
        var field = $scope.getFormPage(i)[formSafeName];
        if (field != null) {
          return field;
        }
      }
      return null;
    }

    // form paging
    $scope.getFormPage = function (pageNumber) {
      return $scope.form["formPage" + pageNumber];
    }
    $scope.isFirstPage = function () {
      return $scope.activePage == 0;
    };
    $scope.isLastPage = function () {
      return $scope.activePage == ($scope.formState.totalPages - 1);
    };
    $scope.isActivePage = function (pageNumber) {
      return $scope.activePage == pageNumber;
    };
    $scope.goToNextPage = function () {
      var formPage = $scope.getFormPage($scope.activePage);
      formPage.showValidationErrors = true;
      if (formPage.$invalid) {
        return;
      }
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
  .directive("requiredFile", function () {
    return {
      require: "ngModel",
      link: function (scope, el, attrs, ctrl) {
        ctrl.$setValidity("requiredFile", el.val() != "");
        el.bind("change", function () {
          ctrl.$setValidity("requiredFile", el.val() != "");
        });
      }
    }
  })
  .directive("maxFileSize", function () {
    return {
      require: "ngModel",
      link: function (scope, el, attrs, ctrl) {
        ctrl.$setValidity("maxFileSize", true);
        el.bind("change", function () {
          var files = el[0].files;
          var valid = files == null || files.length == 0
            ? true
            : files[0].size <= attrs.maxFileSize;
          ctrl.$setValidity("maxFileSize", valid);
        });
      }
    }
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
  })
  .directive("actionField", function () {
    return {
      restrict: "A",
      scope: false,
      link: function (scope, element, attr) {
        scope.$watch("fieldVisibility." + attr.actionField, function (value) {
          if (value == undefined) {
            return;
          }
          element[value ? "removeClass" : "addClass"]("ng-hide");
        }, true);
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
  return (fieldValue + "" || "").toLowerCase() != (rule.condition.expectedFieldValue + "" || "").toLowerCase();
});
// - "field value is" condition (negation of "field value is not" condition):
addFormEditorCondition("core.fieldvalueis", function (rule, fieldValue, formData) {
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
