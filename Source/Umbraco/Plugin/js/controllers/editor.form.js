angular.module("umbraco").controller("FormEditor.Editor.FormController", ["$scope", "$filter", "assetsService", "dialogService", "angularHelper", "contentResource", "formEditorPropertyEditorResource", "formEditorPropertyEditorFieldValidator", "formEditorLocalizationService",
  function ($scope, $filter, assetsService, dialogService, angularHelper, contentResource, formEditorPropertyEditorResource, formEditorPropertyEditorFieldValidator, formEditorLocalizationService) {
    assetsService.loadCss("/App_Plugins/FormEditor/css/form.css");

    // hide the property label?
    $scope.model.hideLabel = $scope.model.config.hideLabel == 1;
    // confirm row and field deletes?
    $scope.model.confirmDelete = $scope.model.config.confirmDelete == 1;
    // is validation enabled?
    $scope.model.enableValidation = $scope.model.config.disableValidation != 1;
    // are multiple form pages enabled?
    $scope.model.enablePages = $scope.model.config.enablePages == 1;

    $scope.emailTemplates = { notification: $scope.model.config.notificationEmailTemplate, confirmation: $scope.model.config.confirmationEmailTemplate };

    // initialize default model if applicable
    $scope.model.value = $scope.model.value || defaultValue();

    // this is for backwards compatability with v0.10.0.1 - should be removed at some point
    if ($scope.model.value.pages == null) {
      $scope.model.value.pages = [
        {
          rows: $scope.model.value.rows || [],
          title: ""
        }
      ];
    }
    $scope.model.value.rows = null;

    // this is for storing the newly added fields, so we know which fields need a warning when they're renamed
    $scope.newlyAddedFields = [];

    $scope.model.successPage = null;
    if ($scope.model.value.successPageId > 0) {
      contentResource.getById($scope.model.value.successPageId).then(
        // success
        function (data) {
          $scope.model.successPage = {
            name: data.name,
            id: data.id,
            cssClass: "icon " + data.icon
          };
        },
        // error
        function (data) {
          // ignore for now
        }
      );
    }

    $scope.model.config.fieldTypes = [];
    $scope.model.config.conditionTypes = [];

    // get the available field types
    formEditorPropertyEditorResource.getAllFieldTypes().then(function (data) {
      $scope.model.config.fieldTypes = data;
    });
    // get the available validation condition types
    formEditorPropertyEditorResource.getAllConditionTypes().then(function (data) {
      $scope.model.config.conditionTypes = data;
    });

    function defaultValue() {
      return {
        rows: []
      };
    }

    function getCellLayout(row, cell) {
      var rowLayout = getRowLayout(row.alias);
      if (rowLayout == null) {
        return;
      }
      return rowLayout.cellLayouts[row.cells.indexOf(cell)];
    }

    $scope.cellWidth = function (row, cell) {
      var rowLayout = getRowLayout(row.alias);
      if (rowLayout == null) {
        return 0;
      }
      var cellLayout = getCellLayout(row, cell);
      if (cellLayout == null) {
        return 0;
      }
      // allocate 2% width for the row trash can
      return (cellLayout.width - (2 / rowLayout.cellLayouts.length));
    }

    $scope.cellAlias = function (row, cell) {
      var cellLayout = getCellLayout(row, cell);
      if (cellLayout == null) {
        return 0;
      }
      return cellLayout.width;
    }

    function getRowLayout(alias) {
      return _.find($scope.model.config.rowLayouts, function (r) {
        return r.alias === alias;
      });
    }

    function pick(type, options, callback, orderBy) {
      dialogService.open({
        dialogData: {
          type: type,
          options: options,
          orderBy: orderBy
        },
        template: "formEditor.compositionPicker.html",
        callback: callback
      });
    }

    $scope.pickRow = function (page) {
      // if we only have one row, let's not bother the user with the dialog
      if ($scope.model.config.rowLayouts.length == 1) {
        $scope.addRow(page, $scope.model.config.rowLayouts[0].alias);
        return;
      }
      if (!$scope.rowTypeOptions) {
        var options = [];
        _.each($scope.model.config.rowLayouts, function (rowLayout) {
          formEditorLocalizationService.localize("composition.row." + rowLayout.alias, rowLayout.prettyName).then(function (value) {
            options.push({
              name: value,
              value: rowLayout.alias,
              iconPath: $scope.pathToRowFile(rowLayout.icon)
            });
          });
        });
        // the composition picker expects groups of options, so let's add all row types to one group
        $scope.rowTypeOptions = [
          {
            title: "",
            options: options
          }
        ];
      }
      pick("row", $scope.rowTypeOptions, function (alias) {
        $scope.addRow(page, alias);
      });
    }

    $scope.addPage = function() {
      $scope.model.value.pages.push({
        rows: [],
        title: ""
      });
    }

    $scope.addRow = function (page, alias) {
      var rowLayout = getRowLayout(alias);
      if (rowLayout == null) {
        return;
      }

      var row = {
        alias: alias,
        cells: []
      };

      for (var i = 0; i < rowLayout.cellLayouts.length; i++) {
        var cellLayout = rowLayout.cellLayouts[i];
        row.cells.push({
          alias: cellLayout.alias,
          fields: []
        });
      }
      page.rows.push(row);
      $scope.setDirty();
    }

    function getFieldType(fieldType) {
      return _.find($scope.model.config.fieldTypes, function (f) {
        return f.type === fieldType;
      });
    }

    $scope.removePage = function (page) {
      if ($scope.model.confirmDelete) {
        formEditorLocalizationService.localize("composition.page.deleteConfirmation", "Are you sure you want to delete this page?").then(function (value) {
          if (confirm(value)) {
            deletePage(page);
          }
        });
      }
      else {
        deletePage(page);
      }
    }

    function deletePage(page) {
      // deleteRow() modifies the rows collection on page, so we need to extract all first rows to delete them one by one
      var containedRows = [];
      _.each(page.rows, function (row) {
        containedRows.push(row);
      });
      _.each(containedRows, function (row) {
        deleteRow(row, page);
      });

      var index = $scope.model.value.pages.indexOf(page);
      $scope.model.value.pages.splice(index, 1);
    }

    $scope.removeRow = function (row, page) {
      if ($scope.model.confirmDelete) {
        formEditorLocalizationService.localize("composition.row.deleteConfirmation", "Are you sure you want to delete this row?").then(function (value) {
          if (confirm(value)) {
            deleteRow(row, page);
          }
        });
      }
      else {
        deleteRow(row, page);
      }
    }

    function deleteRow(row, page) {
      var index = page.rows.indexOf(row);
      page.rows.splice(index, 1);

      var containedFields = [];
      _.each(row.cells, function (cell) {
        _.each(cell.fields, function (field) {
          containedFields.push(field);
        });
      });
      deleteFields(containedFields);
    }

    $scope.getFieldName = function (field) {
      if (field.name) {
        return field.name;
      }
      else if (field.text) {
        // no name... does the field have a text property? e.g. heading, paragraph
        return field.text;
      }
      // default to the field pretty name
      return field.prettyName;
    }

    $scope.pickField = function (cell) {
      if (!$scope.fieldTypeOptions) {
        // lazy load field type groups
        var fieldTypeGroups;
        if ($scope.model.config.fieldTypeGroups && $scope.model.config.fieldTypeGroups.length) {
          // only display the predefined field type groups
          fieldTypeGroups = $scope.model.config.fieldTypeGroups;
        }
        else {
          // display all fields in one group
          var fieldTypes = $scope.model.config.fieldTypes;

          // TODO: remove this once the obsoleted "disallowed field types" prevalue is removed entirely
          if ($scope.model.config.disallowedFieldTypes && $scope.model.config.disallowedFieldTypes.length > 0) {
            // filter out any disallowed field types (as per data type config)
            fieldTypes = $filter("filter")(fieldTypes, function (value, index, array) {
              return $scope.model.config.disallowedFieldTypes.indexOf(value.type) < 0;
            });
          }

          var fieldTypeGroup = {
            name: null,
            fieldTypes: []
          };
          _.each(fieldTypes, function (fieldType) {
            fieldTypeGroup.fieldTypes.push({
              type: fieldType.type,
              prettyName: fieldType.prettyName
            });
          });

          fieldTypeGroups = [fieldTypeGroup];
        }

        $scope.fieldTypeOptions = [];
        _.each(fieldTypeGroups, function (fieldTypeGroup) {
          var optionGroup = {
            title: fieldTypeGroup.name,
            options: []
          };
          $scope.fieldTypeOptions.push(optionGroup);
          _.each(fieldTypeGroup.fieldTypes, function (fieldType) {
            var ft = getFieldType(fieldType.type);
            if (ft != null) {
              formEditorLocalizationService.localize("composition.field." + fieldType.type, fieldType.prettyName).then(function (value) {
                optionGroup.options.push({
                  name: value,
                  value: ft.type,
                  iconPath: $scope.pathToFieldFile(ft.icon)
                });
              });
            }
          });
        });
      }

      pick("field", $scope.fieldTypeOptions, function (fieldType) {
        $scope.addField(fieldType, cell);
      }, "");
    }

    $scope.addField = function (fieldType, cell) {
      var field = angular.copy(getFieldType(fieldType));
      // localize the field type - by default use the field type pretty name as field name
      formEditorLocalizationService.localize("composition.field." + field.type, field.prettyName).then(function (value) {
        if (field.hasOwnProperty("name") && field.name == null) {
          field.name = value;
        }
          // does the field have a text property? e.g. heading, paragraph
        else if (field.hasOwnProperty("text") && field.text == null) {
          field.text = value;
        }
        cell.fields.push(field);
        $scope.newlyAddedFields.push(field);
        $scope.clearFieldCache();
        formEditorPropertyEditorFieldValidator.registerFields($scope.allFields());
        $scope.editField(field);

      });
    }

    $scope.editField = function (field) {
      // always set dirty when opening the edit dialog because we can't react properly to it closing
      $scope.setDirty();

      dialogService.open({
        dialogData: {
          field: field,
          warnWhenRenaming: _.contains($scope.newlyAddedFields, field) == false
        },
        template: $scope.pathToFieldFile(field.view),
        callback: function (field) {
        }
      });
    }

    $scope.removeField = function (field, cell) {
      if ($scope.model.confirmDelete) {
        formEditorLocalizationService.localize("composition.field.deleteConfirmation", "Are you sure you want to delete this field?").then(function (value) {
          if (confirm(value)) {
            deleteField(field, cell);
          }
        });
      }
      else {
        deleteField(field, cell);
      }
    }

    function deleteField(field, cell) {
      var index = cell.fields.indexOf(field);
      cell.fields.splice(index, 1);

      deleteFields([field]);
    }

    function deleteFields(fields) {
      _.each($scope.model.value.validations, function (validation) {
        validation.rules = $filter("filter")(validation.rules, function (rule, index, array) {
          return fields.indexOf(rule.field) < 0;
        });
      });

      $scope.clearFieldCache();
      formEditorPropertyEditorFieldValidator.registerFields($scope.allFields());
      $scope.setDirty();
    }

    $scope.isInvalidField = function (field) {
      return formEditorPropertyEditorFieldValidator.validateField(field) == false;
    }

    $scope.allValueFields = function () {
      if (!$scope._allValueFieldsCache) {
        $scope._allValueFieldsCache = $filter("filter")($scope.allFields(), { isValueField: true });
      }
      return $scope._allValueFieldsCache;
    }

    $scope.clearFieldCache = function () {
      $scope._allFieldsCache = undefined;
      $scope._allValueFieldsCache = undefined;
    }
    $scope.clearFieldCache();

    $scope.allFields = function () {
      if (!$scope._allFieldsCache) {
        $scope._allFieldsCache = [];
        _.each($scope.model.value.pages, function(page) {
          _.each(page.rows, function (row) {
            _.each(row.cells, function (cell) {
              _.each(cell.fields, function (field) {
                $scope._allFieldsCache.push(field);
              });
            });
          });
        });
      }
      return $scope._allFieldsCache;
    }
    formEditorPropertyEditorFieldValidator.registerFields($scope.allFields());

    $scope.allFieldNames = function () {
      var fieldNames = [];
      _.each($scope.allFields(), function (field) {
        if (formEditorPropertyEditorFieldValidator.isNamedField(field)) {
          fieldNames.push(field.name);
        }
      });
      return fieldNames;
    }

    $scope.pathToFieldFile = function (file) {
      return formEditorPropertyEditorResource.pathToFieldFile(file);
    }

    $scope.pathToRowFile = function (file) {
      return formEditorPropertyEditorResource.pathToRowFile(file);
    }

    $scope.pathToConditionFile = function (file) {
      return formEditorPropertyEditorResource.pathToConditionFile(file);
    }

    $scope.pickSuccessPage = function () {
      dialogService.contentPicker({
        multiPicker: false,
        callback: function (data) {
          $scope.model.value.successPageId = data.id;
          $scope.model.successPage = {
            name: data.name,
            id: data.id,
            cssClass: "icon " + data.icon
          };
          $scope.setDirty();
        }
      });
    }

    $scope.removeSuccessPage = function () {
      $scope.model.successPage = null;
      $scope.model.value.successPageId = 0;
      $scope.setDirty();
    }

    $scope.sortableOptionsPage = {
      axis: "y",
      cursor: "move",
      //handle: ".form-page",
      update: function (ev, ui) {
        $scope.setDirty();
      },
      stop: function (ev, ui) {

      }
    };

    $scope.sortableOptionsRow = {
      axis: "y",
      cursor: "move",
      handle: ".form-cells",
      connectWith: ".form-rows",
      items: "li:not(.no-form-rows)",
      update: function (ev, ui) {
        $scope.setDirty();
      },
      stop: function (ev, ui) {

      }
    };

    $scope.sortableOptionsField = {
      cursor: "move",
      handle: ".form-field-content",
      connectWith: ".form-fields",
      update: function (ev, ui) {
        $scope.setDirty();
      },
      stop: function (ev, ui) {

      }
    };

    // helper to force the current form into the dirty state
    $scope.setDirty = function () {
      angularHelper.getCurrentForm($scope).$setDirty();
    }

    //watch for changes
    $scope.$watch("model.value", function (v) {
      $scope.newlyAddedFields = [];
      // wire up the rule fields so any field changes are reflected on the validation rule fields
      $scope.clearFieldCache();
      _.each($scope.model.value.validations, function (validation) {
        _.each(validation.rules, function (rule) {
          var field = _.find($scope.allFields(), function (f) {
            return f.name == rule.field.name;
          });
          if (field != null) {
            rule.field = field;
          }
        });
      });
    });

    // validate all fields when the form submits
    $scope.$on("formSubmitting", function (ev, args) {
      var allFieldsValid = _.find($scope.allFields(), function (field) {
        return formEditorPropertyEditorFieldValidator.validateField(field) == false;
      }) == null;
      angularHelper.getCurrentForm($scope).$setValidity("validation", allFieldsValid);
    });

    $scope.$on("formSubmitted", function (ev, args) {
      // reset the fields collection on validation helper
      formEditorPropertyEditorFieldValidator.registerFields([]);
    });

    // ####################################################
    // ################# validation stuff #################
    // ####################################################

    $scope.model.value.validations = $scope.model.value.validations || [];
    function getConditionType(conditionType) {
      return _.find($scope.model.config.conditionTypes, function (r) {
        return r.type === conditionType;
      });
    }

    $scope.addValidation = function () {
      $scope.pickRule(null, function (rule) {
        $scope.model.value.validations.push({ rules: [rule], errorMessage: "" });
        $scope.setDirty();
      });
    }
    $scope.editRule = function (rule, validation) {
      $scope.pickRule(rule, function (r) {
        if (rule == null) {
          validation.rules.push(r);
        }
        $scope.setDirty();
      });
    }
    $scope.pickRule = function (rule, callback) {
      rule = rule || { field: { name: null }, condition: { type: null } };

      var fields = [];
      _.each($scope.allValueFields(), function (field) {
        fields.push({
          name: field.name,
          iconPath: $scope.pathToFieldFile(field.icon)
        });
      });

      var conditions = [];
      _.each($scope.model.config.conditionTypes, function (condition) {
        formEditorLocalizationService.localize("validation.condition." + condition.type, condition.prettyName).then(function (value) {
          conditions.push({
            name: value,
            type: condition.type,
            iconPath: $scope.pathToConditionFile(condition.icon)
          });
        });
      });

      dialogService.open({
        dialogData: {
          fields: fields,
          conditions: conditions,
          fieldName: rule.field.name,
          conditionType: rule.condition.type
        },
        template: "formEditor.validationPicker.html",
        callback: function (dialogData) {
          var field = _.find($scope.allValueFields(), function (f) {
            return f.name === dialogData.fieldName;
          });

          if (field) {
            var condition = angular.copy(getConditionType(dialogData.conditionType));
            rule.field = field;
            rule.condition = condition;
            callback(rule);
          }
        }
      });
    }
    $scope.removeRule = function (rule, validation) {
      if ($scope.model.confirmDelete) {
        formEditorLocalizationService.localize("validation.condition.deleteRule", "Are you sure you want to delete this rule?").then(function (value) {
          if (confirm(value)) {
            deleteRule(rule, validation);
          }
        });
      }
      else {
        deleteRule(rule, validation);
      }
    }
    $scope.removeValidation = function (validation) {
      if ($scope.model.confirmDelete) {
        formEditorLocalizationService.localize("validation.deleteConfirmation", "Are you sure you want to delete this validation?").then(function (value) {
          if (confirm(value)) {
            deleteValidation(validation);
          }
        });
      }
      else {
        deleteValidation(validation);
      }
    }
    function deleteRule(rule, validation) {
      var index = validation.rules.indexOf(rule);
      validation.rules.splice(index, 1);
      $scope.setDirty();
    }
    function deleteValidation(validation) {
      var index = $scope.model.value.validations.indexOf(validation);
      $scope.model.value.validations.splice(index, 1);
      $scope.setDirty();
    }

  }
]);
