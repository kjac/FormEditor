angular.module("formEditor", [])
  .controller("FormController", ["$scope", "$filter", "$http", "$window", function ($scope, $filter, $http, $window) {
    $scope.formData = {};
    $scope.fileData = {};

    // fetch the validations from global variable
    $scope.validations = _formValidations;
    $scope.invalidValidations = [];

    $scope.submitStatus = "none";

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
        switch (rule.condition.type) {
          case "core.fieldisempty":
            ruleIsFulfilled = (fieldValue == null);
            break;
          case "core.fieldisnotempty":
            ruleIsFulfilled = (fieldValue != null);
            break;
          case "core.fieldvalueisnot":
          case "core.fieldvalueis":
            ruleIsFulfilled = (fieldValue || "").toLowerCase() != (rule.condition.expectedFieldValue || "").toLowerCase();
            if (rule.condition.type == "core.fieldvalueis") {
              ruleIsFulfilled = !ruleIsFulfilled;
            }
            break;
          case "core.fieldvaluesdonotmatch":
          case "core.fieldvaluesmatch":
            var otherFieldValue = $scope.formData[rule.condition.otherFieldName];
            ruleIsFulfilled = (fieldValue || "").toLowerCase() != (otherFieldValue || "").toLowerCase();
            if (rule.condition.type == "core.fieldvaluesmatch") {
              ruleIsFulfilled = !ruleIsFulfilled;
            }
            break;
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