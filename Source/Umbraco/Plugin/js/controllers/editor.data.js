angular.module("umbraco").controller("FormEditor.Editor.DataController", ["$scope", "$filter", "$timeout", "assetsService", "dialogService", "angularHelper", "formEditorPropertyEditorResource", "editorState",
  function ($scope, $filter, $timeout, assetsService, dialogService, angularHelper, formEditorPropertyEditorResource, editorState) {
    assetsService.loadCss("/App_Plugins/FormEditor/css/form.css");

    // hide the property label?
    $scope.model.hideLabel = $scope.model.config.hideLabel == 1;

    // default sorting by date descending 
    $scope.model.sortField = "_created";
    $scope.model.sortDescending = true;

    $scope.model.value = $scope.model.value || {};

    $scope.actionInProgress = false;
    $scope.dataState = "loading";

    $scope.expandedState = {
      visibleFields: {
        expanded: false
      }
    };

    $scope.hasSelection = function () {
      if (!$scope.model.data) {
        return false;
      }
      return _.find($scope.model.data.rows, function (row) {
        return row.selected == true;
      }) != null;
    }

    $scope.selectAll = function () {
      var select = !$scope.allSelected();
      _.each($scope.model.data.rows, function (row) {
        row.selected = select;
      });
    }

    $scope.allSelected = function () {
      if (!$scope.model.data) {
        return false;
      }
      return _.find($scope.model.data.rows, function (row) {
        return row.selected == false;
      }) == null;
    }

    $scope.getSelectedIds = function () {
      var ids = [];
      _.each($scope.model.data.rows, function (row) {
        if (row.selected == true) {
          ids.push(row._id);
        }
      });
      return ids;
    }

    $scope.getDocumentId = function () {
      return editorState.current.id;
    }

    $scope.sort = function (fieldName) {
      if ($scope.model.sortField == fieldName) {
        $scope.model.sortDescending = !$scope.model.sortDescending;
      }
      else {
        $scope.model.sortField = fieldName;
        $scope.model.sortDescending = false;
      }
      $scope.loadPage($scope.model.data.currentPage);
    }

    $scope.goToPage = function (page) {
      if (page <= 0 || page > $scope.model.data.totalPages || page == $scope.model.data.currentPage) {
        return;
      }

      $scope.loadPage(page);
    }

    $scope.loadPage = function (page) {
      $scope.actionInProgress = true;
      formEditorPropertyEditorResource.getData(editorState.current.id, page, $scope.model.sortField, $scope.model.sortDescending, $scope.model.searchQuery).then(function (data) {

        if (data == null || data.rows == null || data.rows.length == 0) {
          $scope.model.data = null;
          $scope.dataState = $scope.model.searchQuery ? $scope.dataState : "no-data";
          $scope.actionInProgress = false;
          return;
        }

        _.each(data.rows, function (row) {
          row.selected = false;
          // for reasons unknown the view messes up in-view date filters on documet save, so we'll execute the filters on load and use local properties per row
          row._createdDateShort = $filter("date")(row._createdDate, "yyyy-MM-dd");
          row._createdDateLong = $filter("date")(row._createdDate, "yyyy-MM-dd HH:mm:ss");
        });

        // make sure we don't end up with a gazillion pagination links if we have lots of submissions
        var start, end;
        var maxPages = 10;
        if (data.totalPages > maxPages) {
          start = page - maxPages / 2;
          if (start < 1) {
            start = 1;
          }
          end = start + maxPages;
          if (end > data.totalPages) {
            end = data.totalPages;
            start = end - maxPages;
          }
        }
        else {
          start = 1;
          end = data.totalPages;
        }
        data.pages = [];
        for (var i = start; i <= end; i++) {
          data.pages.push(i);
        }

        $scope.supportsSearch = data.supportsSearch;
        $scope.supportsStatistics = data.supportsStatistics;
        $scope.supportsApproval = data.supportsApproval;
        $scope.actionInProgress = false;
        $scope.dataState = "data";
        $scope.model.data = data;

        // default to all fields visible
        if (!$scope.model.value.visibleFields) {
          $scope.model.value.visibleFields = [];
          _.each($scope.model.data.fields, function (field) {
            $scope.model.value.visibleFields.push(field.name);
          });
        }
        // clean up any fields that have been deleted from the form
        $scope.model.value.visibleFields = $filter("filter")($scope.model.value.visibleFields, function (fieldName, index, array) {
          return _.find($scope.model.data.fields, function (field) {
            return field.name == fieldName;
          }) != null;
        });
        $scope.actionInProgress = false;
      });
    }

    $scope.deleteSelected = function () {
      var ids = $scope.getSelectedIds();
      if (ids.length == 0) {
        return;
      }

      $scope.actionInProgress = true;

      formEditorPropertyEditorResource.deleteData(editorState.current.id, ids).then(function (data) {
        $scope.loadPage($scope.model.data.currentPage);
      });
    }

    $scope.viewEntry = function (index) {
      if ($scope.hasHiddenFields() == false) {
        return;
      }
      dialogService.open({
        dialogData: {
          row: $scope.model.data.rows[index],
          fields: $scope.model.data.fields
        },
        template: "data.viewEntry.html"
      });
    }

    $scope.selectFields = function () {
      dialogService.open({
        dialogData: {
          fieldConfigurations: _.map($scope.model.data.fields, function(field) {
            return {
              field: field,
              selected: $scope.isFieldVisible(field)
            };
          })
        },
        template: "data.selectFields.html",
        callback: function (dialogData) {
          $scope.model.value.visibleFields = _.map(_.where(dialogData.fieldConfigurations, { selected: true }), function(fieldConfiguration) {
            return fieldConfiguration.field.name;
          });
        }
      });
    }

    $scope.isFieldVisible = function (field) {
      return !$scope.model.value.visibleFields || $scope.model.value.visibleFields.indexOf(field.name) >= 0;
    }

    $scope.hasHiddenFields = function () {
      if (!$scope.model.data) {
        return false;
      }
      return $scope.model.value.visibleFields && $scope.model.value.visibleFields.length !== $scope.model.data.fields.length;
    }

    // helper to force the current form into the dirty state
    $scope.setDirty = function () {
      angularHelper.getCurrentForm($scope).$setDirty();
    }

    $scope.searchPromise = null;
    $scope.search = function () {
      if ($scope.searchPromise != null) {
        $timeout.cancel($scope.searchPromise);
      }
      $scope.searchPromise = $timeout(function () {
        $scope.loadPage(1);
      }, 600);
    }

    $scope.showStatistics = function () {
      dialogService.open({
        dialogData: {},
        template: "data.statistics.html",
        modalClass: "umb-modal stats-modal"
      });
    }

    $scope.toggleApproval = function (row) {
      if (row._actionInProgress) {
        return;
      }
      row._actionInProgress = true;
      var newApprovalState = row._approval === "none" ? "approved" : "none";
      formEditorPropertyEditorResource.setApprovalState(editorState.current.id, newApprovalState, row._id).then(function (data) {
        if (data && data.newApprovalState) {
          row._approval = data.newApprovalState;
          row._actionInProgress = false;
        }
      });
    }

    $scope.loadPage(1);
  }
]);
