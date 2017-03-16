angular.module("umbraco").controller("FormEditor.Config.WebServiceController", ["$scope", "$filter", "assetsService", "angularHelper", 
  function ($scope, $filter, assetsService, angularHelper) {
    assetsService.loadCss("/App_Plugins/FormEditor/css/form.css");

    $scope.model.value = $scope.model.value || defaultValue();

    function defaultValue() {
      return {
        url: "",
        userName: "",
        password: ""
      }
    }

    // helper to force the current form into the dirty state
    $scope.setDirty = function () {
      angularHelper.getCurrentForm($scope).$setDirty();
    }
  }
]);
