$(function () {
  var $form = $("form");

  // validate all named fields and all validations on submit
  $form.submit(function (event) {
    var hasError = false;

    // validate all named fields
    $("[name]", $form).each(function (index, input) {
      var isValid = validateField(input);
      showHideValidationErrorForField(input, isValid);
      if (isValid == false) {
        hasError = true;
      }
    });

    // validate all validations
    var validationErrors = $("#validationErrors", $form);
    var validationErrorsList = $("#validationErrorsList", $form);
    validationErrors.addClass("hide");
    validationErrorsList.empty();
    // traverse the validations (in the global variable _formValidations)
    $(_formValidations).each(function (index, validation) {
      if (validateValidation(validation) == false) {
        hasError = true;
        validationErrors.removeClass("hide");
        validationErrorsList.append("<li>" + validation.errorMessage + "</li>");
      }
    });

    if (hasError == true) {
      event.preventDefault();
    }
  });

  // validate all named fields on value change
  $("[name]", $form).change(function (event) {
    var input = event.target;
    var isValid = validateField(input);
    showHideValidationErrorForField(input, isValid);
  });

  // validate a single input field
  function validateField(input) {
    var isValid = false;

    // get all required fields grouped by name (in case of a group of fields with the same name, e.g. checkbox group)
    var group = $("[name='" + input.name + "']", $form);
    group.each(function (index, input) {
      // group is valid if one or more inputs in the group are valid
      if (input.validity.valid) {
        isValid = true;
        // no need to continue the loop
        return false;
      }
    });

    return isValid;
  }

  // show/hide the validation error for a single input field
  function showHideValidationErrorForField(input, isValid) {
    var validationError = $(".validation-error", $(input).closest(".form-group"));
    if (isValid) {
      validationError.addClass("hide");
    }
    else {
      validationError.removeClass("hide");
    }
  }

  // validate a validation (usually cross field validation)
  function validateValidation(validation) {
    var isValid = false;
    $(validation.rules).each(function (index, rule) {
      // get all fields that matches the rule field name (in case of a group of fields with the same name, e.g. checkbox group)
      var group = $("[name=" + rule.field.formSafeName + "]");

      // figure out the value of the field group
      var fieldValue = null;
      switch (group.attr("type")) {
        case "checkbox":
          // checkbox/checkbox group: field value is the value of all checked checkboxes in the group (if any)
          fieldValue = group.filter(":checked").map(function () { return this.value; }).toArray();
          break;
        case "radio":
          // radio button group: field value is the value of the checked radio button (if any)
          fieldValue = group.filter(":checked").val();
          break;
        default:
          // default: field value is the value of the first field in the group (there is probably only one field in the group)
          fieldValue = group.val();
      }
      // treat empty and undefined values as null values for a cleaner validation below
      if (fieldValue == undefined || fieldValue == "") {
        fieldValue = null;
      }
      // concat array values (e.g. select boxes and checkbox groups) to comma separated strings
      if ($.isArray(fieldValue)) {
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
        case "core.fieldvalueis":
          ruleIsFulfilled = (fieldValue != null && fieldValue.toLowerCase() == (rule.condition.expectedFieldValue || "").toLowerCase());
          break;
        case "core.fieldvalueisnot":
          ruleIsFulfilled = (fieldValue != null && fieldValue.toLowerCase() != (rule.condition.expectedFieldValue || "").toLowerCase());
          break;
        case "core.fieldvaluesdonotmatch":
        case "core.fieldvaluesmatch":
          var otherField = $("[name='" + rule.condition.otherFieldName + "']", $form);
          ruleIsFulfilled = (fieldValue != null && otherField != null && fieldValue.toLowerCase() != otherField.val().toLowerCase());
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
  }
});