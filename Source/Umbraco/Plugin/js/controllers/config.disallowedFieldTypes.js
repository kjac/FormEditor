angular.module("umbraco").controller("FormEditor.Config.DisallowedFieldTypesController", ["$scope", "$filter", "assetsService", "formEditorPropertyEditorResource",
  function ($scope, $filter, assetsService, formEditorPropertyEditorResource) {
    assetsService.loadCss("/App_Plugins/FormEditor/css/form.css");

    $scope.model.value = $scope.model.value || [];
    $scope.model.fieldTypes = [];

    function isDisallowedFieldType(fieldType) {
      return $scope.model.value.indexOf(fieldType.type) >= 0;
    }

    $scope.disallowedFieldTypes = function () {
      return $filter("filter")($scope.model.fieldTypes, function (value, index, array) {
        return isDisallowedFieldType(value);
      });
    }

    $scope.availableFieldTypes = function () {
      return $filter("filter")($scope.model.fieldTypes, function (value, index, array) {
        return isDisallowedFieldType(value) === false;
      });
    }

    $scope.addFieldType = function (fieldType) {
      if (fieldType == null) {
        return;
      }
      if (isDisallowedFieldType(fieldType)) {
        return;
      }
      $scope.model.value.push(fieldType.type);
    }

    $scope.removeFieldType = function (fieldType) {
      if (isDisallowedFieldType(fieldType) === false) {
        return;
      }

      $scope.model.value.splice($scope.model.value.indexOf(fieldType.type), 1);
    }

    formEditorPropertyEditorResource.getAllFieldTypes().then(function (data) {
      $scope.model.fieldTypes = data;
    });
  }
]);
