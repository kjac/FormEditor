angular.module("umbraco").controller("FormEditor.Dashboard.Controller", ["$scope", "$http", "$timeout", "assetsService",
  function ($scope, $http, $timeout, assetsService) {
    assetsService.loadCss("/App_Plugins/FormEditor/css/form.css");

    $scope.dataState = "";
    $scope.model = {};
    $scope.working = false;

    $scope.search = function () {
      if (!$scope.model.searchQuery) {
        return;
      }
      $scope.dataState = "loading";
      $scope.working = true;
      $http.get("/umbraco/backoffice/FormEditorApi/Dashboard/SearchAll", { params: { searchQuery: $scope.model.searchQuery } })
        .then(
          function (response) {
            if (response.data && response.data.length) {
              $scope.dataState = "data";
              $scope.model.results = response.data;
            } else {
              $scope.dataState = "no-data";
              $scope.model.results = null;
            }
            $scope.working = false;
          },
          function (response) {
            console.log("TODO: got error", response);
            $scope.working = false;
          }
        );
    }


    $scope.getResult = function (contentId) {
      if (!$scope.model.results) {
        return null;
      }
      return _.find($scope.model.results,
        function (result) {
          return result.contentId == contentId;
        });
    }

    $scope.hasSelection = function (contentId) {
      var result = $scope.getResult(contentId);
      if (result == null) {
        return false;
      }

      return _.find(result.rows, function (row) {
        return row.selected == true;
      }) != null;
    }

    $scope.selectAll = function (contentId) {
      var result = $scope.getResult(contentId);
      if (result == null) {
        return;
      }
      var select = !$scope.allSelected(contentId);
      _.each(result.rows, function (row) {
        row.selected = select;
      });
    }

    $scope.allSelected = function (contentId) {
      var result = $scope.getResult(contentId);
      if (result == null) {
        return false;
      }
      return _.find(result.rows, function (row) {
        return !row.selected;
      }) == null;
    }

    $scope.deleteSelected = function (contentId) {
      var result = $scope.getResult(contentId);
      if (result == null) {
        return;
      }

      var ids = [];
      _.each(result.rows, function (row) {
        if (row.selected == true) {
          ids.push(row.id);
        }
      });
      if (ids.length == 0) {
        return;
      }

      if (confirm("Are you sure you want to delete " + (ids.length > 1 ? "the " + ids.length + " selected submissions?" : "the selected submission?")) == false) {
        return;
      }

      $scope.working = true;

      $http.post("/umbraco/backoffice/FormEditorApi/Dashboard/Delete", { contentId: contentId, rowIds: ids })
        .then(
          function (response) {
            result.rows = _.filter(result.rows,
              function (row) {
                return !row.selected;
              });
            $scope.working = false;
          },
          function (response) {
            console.log("TODO: got error", response);
            $scope.working = false;
          }
        );
    }
  }
]);
