angular.module("umbraco").controller("FormEditor.Editor.StatisticsController", ["$scope", "$timeout", "assetsService", "formEditorPropertyEditorResource", "editorState",
    function ($scope, $timeout, assetsService, formEditorPropertyEditorResource, editorState) {

      $scope.fields = null;
      $scope.loading = true;

      // for the time being we only have field value frequency statistics. hopefully this will be extended over time.
      formEditorPropertyEditorResource.getFieldValueFrequencyStatistics(editorState.current.id).then(function (data) {
        $scope.fields = data.fields;
        $scope.totalRows = data.totalRows;

        // make sure there is actually any statistics to show before loading a whole bunch of graph stuff
        if ($scope.fields != null && $scope.fields.length) {
          assetsService.loadJs("https://www.gstatic.com/charts/loader.js").then(function () {
            if (formEditorPropertyEditorResource.googleChartsLoaded == false) {
              google.charts.load("current", { "packages": ["corechart", "bar"] });
              google.charts.setOnLoadCallback(googleChartsLoadCallback);
            }
            else {
              googleChartsLoadCallback();
            }
          });
        }
      });

      function googleChartsLoadCallback() {
        formEditorPropertyEditorResource.googleChartsLoaded = true;
        _.each($scope.fields, function (field) {
          field.chartData = [["", ""]]; // legend header - leave empty
          _.each(field.values, function (value) {
            field.chartData.push([value.value, value.frequency]);
          });
        });
        $timeout(function () {
          drawCharts();
        }, 200);
      }

      function drawCharts() {
        _.each($scope.fields, function (field) {
          var data = google.visualization.arrayToDataTable(field.chartData);

          var chart, options;
          var chartContainer = document.getElementById(field.formSafeName);

          if (field.multipleValuesPerEntry) {
            // use bar charts for multiselect fields
            options = {
              chartArea: { width: "60%" },
              hAxis: {
                minValue: 0
              },
              vAxis: {
              },
              legend: "none"
            };
            chart = new google.visualization.BarChart(chartContainer);
          }
          else {
            // use pie charts for singleselect fields
            options = {
              chartArea: { left: 20, top: 20 },
              legend: {
                position: "bottom"
              }
            };
            chart = new google.visualization.PieChart(chartContainer);
          }

          chart.draw(data, options);
        });
        $scope.loading = false;
      }
    }
]);
