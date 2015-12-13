angular.module("umbraco").controller("FormEditor.Config.RowLayoutsController", ["$scope", "assetsService", "angularHelper", "formEditorPropertyEditorResource",
  function ($scope, assetsService, angularHelper, formEditorPropertyEditorResource) {
    assetsService.loadCss("/App_Plugins/FormEditor/css/form.css");

    $scope.model.value = $scope.model.value || defaultValue();
    $scope.model.rowIcons = [];

    formEditorPropertyEditorResource.getRowIcons().then(function (data) {
      $scope.model.rowIcons = data;
    });

    function defaultValue() {
      return [{
        alias: "two-column",
        icon: "two-column.png",
        prettyName: "Two columns",
        expanded: true,
        cellLayouts: [
            {
              alias: "col-md-6",
              width: 50
            },
            {
              alias: "col-md-6",
              width: 50
            }
        ]
      }];
    }

    $scope.addRowLayout = function () {
      $scope.model.value.push({
        alias: "one-column",
        icon: "one-column.png",
        prettyName: "One column",
        expanded: true,
        cellLayouts: []
      });
      var newRowLayout = $scope.model.value[$scope.model.value.length - 1];
      $scope.addCellLayout(newRowLayout);
    }

    $scope.deleteRowLayout = function (rowLayout) {
      if (confirm("Are you sure you want to delete this row?")) {
        $scope.model.value.splice($scope.model.value.indexOf(rowLayout), 1);
      }
    }

    $scope.deleteCellLayout = function (rowLayout, cellLayout) {
      if (confirm("Are you sure you want to delete this cell?")) {
        rowLayout.cellLayouts.splice(rowLayout.cellLayouts.indexOf(cellLayout), 1);
      }
    }

    $scope.addCellLayout = function (rowLayout) {
      var width = 100 - totalCellWidth(rowLayout);
      var alias = width == 100
        ? "col-md-12"
        : width == 75
          ? "col-md-9"
          : width == 50
            ? "col-md-6"
            : width == 25
              ? "col-md-3"
              : width >= 32 && width <= 35
                  ? "col-md-4"
                  : width >= 65 && width <= 68
                    ? "col-md-8"
                    : "alias";
      rowLayout.cellLayouts.push({
        alias: alias,
        width: width
      });
    }

    function totalCellWidth(rowLayout) {
      var totalWidth = 0.0;
      _.each(rowLayout.cellLayouts, function (c) {
        totalWidth += c.width;
      });

      return totalWidth.toFixed(2);
    }

    $scope.isValidCellLayout = function (rowLayout) {
      if (rowLayout.cellLayouts.length == 0) {
        return false;
      }

      return totalCellWidth(rowLayout) == 100;
    }

    // validate all row layouts when the form submits
    $scope.$on("formSubmitting", function (ev, args) {
      var allRowLayoutsValid = _.find($scope.model.value, function (rowLayout) {
        return $scope.isValidCellLayout(rowLayout) == false
      }) == null;
      angularHelper.getCurrentForm($scope).$setValidity("validation", allRowLayoutsValid);
    });

    $scope.sortableOptionsRowLayout = {
      axis: "y",
      cursor: "move",
      handle: ".row-layout",
      update: function (ev, ui) {
        $scope.setDirty();
      },
      stop: function (ev, ui) {

      }
    };

    $scope.sortableOptionsCellLayout = {
      axis: "y",
      cursor: "move",
      handle: ".cell-layout",
      update: function (ev, ui) {
        $scope.setDirty();
      },
      stop: function (ev, ui) {

      }
    };

    $scope.pathToRowFile = function (file) {
      return formEditorPropertyEditorResource.pathToRowFile(file);
    }

    // helper to force the current form into the dirty state
    $scope.setDirty = function () {
      angularHelper.getCurrentForm($scope).$setDirty();
    }
  }
]);
