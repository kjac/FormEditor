angular.module("umbraco").controller("FormEditor.Config.EmailTemplatesController", ["$scope", "assetsService", "formEditorPropertyEditorResource",
  function ($scope, assetsService, formEditorPropertyEditorResource) {
    assetsService.loadCss("/App_Plugins/FormEditor/css/form.css");

    $scope.model.value = $scope.model.value || null;
    $scope.model.templates = [];

    formEditorPropertyEditorResource.getEmailTemplates().then(function (data) {
      $scope.model.templates = data;
    });
  }
]);
