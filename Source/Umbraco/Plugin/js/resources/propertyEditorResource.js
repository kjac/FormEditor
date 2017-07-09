angular.module("umbraco.resources").factory("formEditorPropertyEditorResource", ["$q", "$http", "$timeout", "umbRequestHelper",
  function ($q, $http, $timeout, umbRequestHelper) {
    return {
      getAllFieldTypes: function () {
        return umbRequestHelper.resourcePromise(
            $http.get("/umbraco/backoffice/FormEditorApi/PropertyEditor/GetAllFieldTypes"), "Could not retrieve field types"
        );
      },
      getAllConditionTypes: function () {
        return umbRequestHelper.resourcePromise(
            $http.get("/umbraco/backoffice/FormEditorApi/PropertyEditor/GetAllConditionTypes"), "Could not retrieve condition types"
        );
      },
      getEmailTemplates: function () {
        return umbRequestHelper.resourcePromise(
            $http.get("/umbraco/backoffice/FormEditorApi/PropertyEditor/GetEmailTemplates"), "Could not retrieve email templates"
        );
      },
      getRowIcons: function () {
        return umbRequestHelper.resourcePromise(
            $http.get("/umbraco/backoffice/FormEditorApi/PropertyEditor/GetRowIcons"), "Could not retrieve row icons"
        );
      },
      getData: function (documentId, page, sortField, sortDescending, searchQuery) {
        return umbRequestHelper.resourcePromise(
            $http.get("/umbraco/backoffice/FormEditorApi/PropertyEditor/GetData/" + documentId, { params: { page: page, sortField: sortField, sortDescending: sortDescending, searchQuery: searchQuery } }), "Could not retrieve data"
        );
      },
      getFieldValueFrequencyStatistics: function (documentId) {
        return umbRequestHelper.resourcePromise(
            $http.get("/umbraco/backoffice/FormEditorApi/PropertyEditor/GetFieldValueFrequencyStatistics/" + documentId), "Could not retrieve field value frequency statistics"
        );
      },
      deleteData: function (documentId, ids) {
        // posting all IDs for deletion here in one bulk operation .. not quite the REST way but more efficient this way.
        return umbRequestHelper.resourcePromise(
            $http.post("/umbraco/backoffice/FormEditorApi/PropertyEditor/RemoveData/" + documentId, ids), "Could not delete data"
        );
      },
      setApprovalState: function (documentId, approvalState, rowId) {
        return umbRequestHelper.resourcePromise(
            $http.put("/umbraco/backoffice/FormEditorApi/PropertyEditor/SetApprovalState/" + documentId, { rowId: rowId, approvalState: approvalState }), "Could not set approval state"
        );
      },
      getMediaUrl: function (mediaId) {
        return umbRequestHelper.resourcePromise(
            $http.get("/umbraco/backoffice/FormEditorApi/PropertyEditor/GetMediaUrl/" + mediaId), "Could not retrieve media URL"
        );
      },
      // for future support for views and icons and stuff outside the default location
      pathToFieldFile: function (file) {
        return "/App_Plugins/FormEditor/editor/fields/" + file;
      },
      // for future support for views and icons and stuff outside the default location
      pathToRowFile: function (file) {
        return "/App_Plugins/FormEditor/editor/rows/" + file;
      },
      // for future support for views and icons and stuff outside the default location
      pathToConditionFile: function (file) {
        return "/App_Plugins/FormEditor/editor/conditions/" + file;
      },
      // this indicates if the google charts loader has been executed or not
      googleChartsLoaded: false
    }
  }
]);
