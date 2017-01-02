angular.module("umbraco").controller("FormEditor.Config.TabOrderController", ["$scope", "$filter", "assetsService", "angularHelper", 
  function ($scope, $filter, assetsService, angularHelper) {
    assetsService.loadCss("/App_Plugins/FormEditor/css/form.css");

    var tabs = [
      { name: "Form layout", icon: "icon-layout", id: "layout", visible: true },
      { name: "Validation", icon: "icon-check", id: "validation", visible: true },
      { name: "Actions", icon: "icon-wand", id: "actions", visible: true },
      { name: "Emails", icon: "icon-message", id: "emails", visible: true },
      { name: "Receipt", icon: "icon-document", id: "receipt", visible: true },
      { name: "Limitations", icon: "icon-filter", id: "limitations", visible: true },
      { name: "Submissions", icon: "icon-list", id: "submissions", visible: true }
    ];

    if (!$scope.model.value) {
      $scope.model.value = tabs;
    }
    else {
      _.each(tabs, function (tab) {
        if (_.find($scope.model.value, function (t) { return t.id == tab.id; }) == null) {
          tab.visible = false;
          $scope.model.value.push(tab);
        }
      });
    }

    $scope.sortableOptionsTabs = {
      axis: "y",
      cursor: "move",
      update: function (ev, ui) {
        $scope.setDirty();
      },
      stop: function (ev, ui) {

      }
    };

    $scope.isLastVisibleTab = function (tab) {
      return tab.visible == true && _.where($scope.model.value, { visible: true }).length == 1;
    }

    // helper to force the current form into the dirty state
    $scope.setDirty = function () {
      angularHelper.getCurrentForm($scope).$setDirty();
    }
  }
]);
