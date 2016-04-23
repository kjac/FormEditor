angular.module("umbraco").controller("FormEditor.Config.FieldTypeGroupsController", ["$scope", "assetsService", "angularHelper", "formEditorPropertyEditorResource",
  function ($scope, assetsService, angularHelper, formEditorPropertyEditorResource) {
    assetsService.loadCss("/App_Plugins/FormEditor/css/form.css");

    $scope.model.value = $scope.model.value || defaultValue();
    $scope.model.fieldTypes = [];
    $scope.model.unassignedFieldTypes = [];

    formEditorPropertyEditorResource.getAllFieldTypes().then(function (data) {
      $scope.model.fieldTypes = data;
      loadUnassignedFieldTypes();
    });

    function defaultValue() {
      return [];
    }

    function loadUnassignedFieldTypes() {
      $scope.model.unassignedFieldTypes = [];
      _.each($scope.model.fieldTypes, function (fieldType) {
        if (_.find($scope.model.value, function (fieldTypeGroup) {
          return _.find(fieldTypeGroup.fieldTypes, function (field) {
            return field.type === fieldType.type;
          }) != null;
        }) == null) {
          $scope.model.unassignedFieldTypes.push(fieldType);
        }
      });
    }

    $scope.addFieldTypeGroup = function () {
      $scope.model.value.push({
        name: "Field group",
        expanded: true,
        fieldTypes: []
      });
    }

    $scope.deleteFieldTypeGroup = function (fieldTypeGroup) {
      if (confirm("Are you sure you want to delete this field type group?")) {
        $scope.model.value.splice($scope.model.value.indexOf(fieldTypeGroup), 1);
        loadUnassignedFieldTypes();
      }
    }

    $scope.deleteFieldType = function (fieldType, fieldTypeGroup) {
      fieldTypeGroup.fieldTypes.splice(fieldTypeGroup.fieldTypes.indexOf(fieldType), 1);
      loadUnassignedFieldTypes();
    }

    $scope.availableFieldTypes = function () {
      return $scope.model.fieldTypes;
    };

    $scope.addSelectedFieldType = function (fieldTypeGroup) {
      fieldTypeGroup.fieldTypes.push({ prettyName: $scope.model.selectedFieldType.prettyName, type: $scope.model.selectedFieldType.type });
      $scope.model.selectedFieldType = null;
      loadUnassignedFieldTypes();
    };

    //$scope.$on("formSubmitting", function (ev, args) {
    //});

    $scope.sortableOptionsFieldTypeGroup = {
      axis: "y",
      cursor: "move",
      handle: ".collapsible-block",
      update: function (ev, ui) {
        $scope.setDirty();
      },
      stop: function (ev, ui) {

      }
    };

    $scope.sortableOptionsFieldType = {
      axis: "y",
      cursor: "move",
      handle: ".assigned-field",
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
  }
]);
