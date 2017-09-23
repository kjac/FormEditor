(function ($) {
  $.fn.formEditor = function () {
    this.each(function (idx, el) {
      var $form = $(el);
      var formId = $form.data("form-editor");

      if (typeof formId == "undefined") {
        console.error("Could not find any Form Editor ID in data-form-editor");
        return;
      }

      if (typeof _fe == "undefined" || typeof _fe[formId] == "undefined") {
        console.error("Could not find any Form Editor state for form ID " + formId);
        return;
      }

      $form.formState = _fe[formId];
      $form.activePage = 0;

      // validate all named fields and all validations on submit
      $form.submit(function (event) {
        var isValid = validateForm($form);
        if (isValid == false) {
          event.preventDefault();
        }
      });

      // validate all named fields on value change
      $("[name]", $form).change(function (event) {
        var input = event.target;
        var isValid = validateField(input, $form);
        $.fn.formEditor.showHideValidationErrorForField(input, isValid);
        executeActions($form);
      });

      // form paging
      $(".form-btn-next", $form).click(function (e) {
        if (validateActivePage($form) == false) {
          return;
        }
        if ($form.activePage < formTotalPages($form)) {
          $form.activePage++;
          showActivePage($form);
        }
      });
      $(".form-btn-previous", $form).click(function (e) {
        if ($form.activePage > 0) {
          $form.activePage--;
          showActivePage($form);
        }
      });

      showActivePagingButtons($form);
      executeActions($form);

      // create a global scope access to form validation and submission
      // (this may seem a bit strange and un-jQuery like, but we're doing it this way to use the same pattern in both Async and Sync )
      window.feGlobal = window.feGlobal || [];
      window.feGlobal[formId] = {
        submit: function () { $form.submit(); },
        validate: function() { return validateForm($form); },
        setValue: function (input, value) { setFieldValue(input, value, $form); }
      };
    });

    return this;
  };

  // show/hide the validation error for a single input field
  $.fn.formEditor.showHideValidationErrorForField = function (input, isValid) {
    var validationError = $(".validation-error", $(input).closest(".form-group"));
    if (isValid) {
      validationError.addClass("hide d-none");
    }
    else {
      validationError.removeClass("hide d-none");
    }
  }

  function setFieldValue(input, value, $form) {
    if (input == null) {
      return;
    }
    input.value = value;
    var isValid = validateField(input, $form);
    $.fn.formEditor.showHideValidationErrorForField(input, isValid);
  }

  function validateForm($form) {
    var hasError = false;

    // validate all named fields
    $("[name]", $form).each(function (index, input) {
      var isValid = validateField(input, $form);
      $.fn.formEditor.showHideValidationErrorForField(input, isValid);
      if (isValid == false) {
        hasError = true;
      }
    });

    // validate all validations
    var validationErrors = $("#validationErrors", $form);
    var validationErrorsList = $("#validationErrorsList", $form);
    validationErrors.addClass("hide d-none");
    validationErrorsList.empty();
    // traverse the validations
    $($form.formState.validations).each(function (index, validation) {
      if (validateValidation(validation, $form) == false) {
        hasError = true;
        validationErrors.removeClass("hide d-none");
        validationErrorsList.append("<li>" + validation.errorMessage + "</li>");
      }
    });

    return hasError == false;
  }

  // validate a single input field
  function validateField(input, $form) {
    var isValid = false;

    // get all required fields grouped by name (in case of a group of fields with the same name, e.g. checkbox group)
    var group = $("[name='" + input.name + "']", $form);
    group.each(function (index, input) {
      // group is valid if one or more inputs in the group are valid
      if (input.validity == null || input.validity.valid) {
        isValid = true;
        // no need to continue the loop
        return false;
      }
    });

    return isValid;
  }

  // validate a validation (usually cross field validation)
  function validateValidation(validation, $form) {
    if (!validation.rules || !validation.rules.length) {
      // edge case: validation contains no rules. must be valid.
      return true;
    }
    // a validation fails if all rules are fulfilled
    return validateRules(validation.rules, $form) == false;
  }

  // validate a set of rules
  function validateRules(rules, $form) {
    var allRulesFulfilled = true;
    $(rules).each(function (index, rule) {
      // get all fields that matches the rule field name (in case of a group of fields with the same name, e.g. checkbox group)
      var group = $("[name=" + rule.field.formSafeName + "]", $form);

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
      var conditionCallback = getFormEditorCondition(rule.condition.type);
      if (conditionCallback) {
        try {
          ruleIsFulfilled = conditionCallback(rule, fieldValue, $form);
        }
        catch (err) {
          // log error and continue (and hope the server side validation handles things)
          console.log(err);
        }
      }

      if (ruleIsFulfilled == false) {
        allRulesFulfilled = false;
        // no need to continue the loop
        return false;
      }
    });

    return allRulesFulfilled;
  }
  function executeActions($form) {
    $($form.formState.actions).each(function (index, action) {
      var inputs = $("[name='" + action.field.formSafeName + "']", $form);
      if (inputs && inputs.length) {
        var allRulesFulfilled = validateRules(action.rules, $form);
        $(inputs).each(function (i, input) {
          var formGroup = $(input).closest(".form-group");
          switch (action.task) {
            case "core.showfield":
            case "core.hidefield":
              var show = action.task == "core.showfield";
              if (show == allRulesFulfilled) {
                formGroup.show();
              }
              else {
                formGroup.hide();
              }
              break;
          }
        });
      }
    });
  }

  function formTotalPages($form) {
    return $form.formState.totalPages - 1;
  }
  function validateActivePage($form) {
    var pageIsValid = true;
    $("[name]", $($(".form-page", $form)[$form.activePage])).each(function (index, input) {
      var isValid = validateField(input, $form);
      $.fn.formEditor.showHideValidationErrorForField(input, isValid);
      if (isValid == false) {
        pageIsValid = false;
      }
    });
    return pageIsValid;
  }
  function showActivePage($form) {
    $(".form-page", $form).hide();
    $($(".form-page", $form)[$form.activePage]).show();
    showActivePagingButtons($form);
  }
  function showActivePagingButtons($form) {
    if ($form.activePage == 0) {
      $(".form-btn-previous", $form).hide();
    }
    else {
      $(".form-btn-previous", $form).show();
    }
    if ($form.activePage == formTotalPages($form)) {
      $(".form-btn-next", $form).hide();
    }
    else {
      $(".form-btn-next", $form).show();
    }
  }

}(jQuery));

$(function () {
  $("form[data-form-editor]").formEditor();
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
addFormEditorCondition("core.fieldisnotempty", function (rule, fieldValue, $form) {
  return (fieldValue != null);
});
// - "field is empty" condition (negation of "field is not empty" condition):
addFormEditorCondition("core.fieldisempty", function (rule, fieldValue, $form) {
  return !(getFormEditorCondition("core.fieldisnotempty")(rule, fieldValue, $form));
});
// - "field value is not" condition:
addFormEditorCondition("core.fieldvalueisnot", function (rule, fieldValue, $form) {
  return (fieldValue + "" || "").toLowerCase() != (rule.condition.expectedFieldValue + "" || "").toLowerCase();
});
// - "field value is" condition (negation of "field value is not" condition):
addFormEditorCondition("core.fieldvalueis", function (rule, fieldValue, $form) {
  return !(getFormEditorCondition("core.fieldvalueisnot")(rule, fieldValue, $form));
});
// - "field value does not other field value" condition:
addFormEditorCondition("core.fieldvaluesdonotmatch", function (rule, fieldValue, $form) {
  var otherField = $("[name='" + rule.condition.otherFieldName + "']", $form);
  var otherFieldValue = otherField != null ? otherField.val() : null;
  return (fieldValue || "").toLowerCase() != (otherFieldValue || "").toLowerCase();
});
// - "field value matches other field value" condition (negation of "field value does not other field value" condition):
addFormEditorCondition("core.fieldvaluesmatch", function (rule, fieldValue, $form) {
  return !(getFormEditorCondition("core.fieldvaluesdonotmatch")(rule, fieldValue, $form));
});
