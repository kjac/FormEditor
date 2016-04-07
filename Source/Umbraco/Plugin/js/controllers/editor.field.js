angular.module("umbraco").controller("FormEditor.Editor.FieldController", ["$scope", "formEditorPropertyEditorFieldValidator", "formEditorPropertyEditorResource", "dialogService",
  function ($scope, formEditorPropertyEditorFieldValidator, formEditorPropertyEditorResource, dialogService) {
    $scope.originalFieldName = $scope.dialogData.field.name;
    $scope.addFieldValue = function () {
      $scope.dialogData.field.fieldValues.push({});
    };
    $scope.removeFieldValue = function (index) {
      $scope.dialogData.field.fieldValues.splice(index, 1);
    };
    $scope.setSelectedFieldValue = function (index) {
      if (!$scope.dialogData.field.multiSelect) {
        _.each($scope.dialogData.field.fieldValues, function (fieldValue) {
          if ($scope.dialogData.field.fieldValues.indexOf(fieldValue) != index) {
            fieldValue.selected = false;
          }
        });
      }
    };
    $scope.validateFieldName = function ($event) {
      return formEditorPropertyEditorFieldValidator.validateFieldName($scope.dialogData.field);
    };
    $scope.pickImage = function () {
      dialogService.mediaPicker({
        multiPicker: false,
        callback: function (data) {
          $scope.dialogData.field.mediaId = data.id;
          $scope.loadMediaUrl();
        }
      });
    }
    $scope.loadMediaUrl = function () {
      formEditorPropertyEditorResource.getMediaUrl($scope.dialogData.field.mediaId).then(function (data) {
        //console.log("Got media URL", $scope.dialogData.field.mediaId, data);
        $scope.dialogData.field.mediaUrl = data.url;
      });
    }
    $scope.fieldNameChanged = function() {
      if ($scope.dialogData.warnWhenRenaming) {
        $scope.showRenameWarning = $scope.dialogData.field.name != $scope.originalFieldName;
      }
    }
    if ($scope.dialogData.field.mediaId) {
      $scope.loadMediaUrl();
    }
    $scope.sortableOptionsFieldValues = {
      axis: "y",
      cursor: "move",
      update: function (ev, ui) {
      },
      stop: function (ev, ui) {
      }
    };
  }
]);
