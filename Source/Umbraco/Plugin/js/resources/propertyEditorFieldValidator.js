angular.module("umbraco.resources").factory("formEditorPropertyEditorFieldValidator", [
  function () {
    function propertyEditorFieldValidator() {
      this.fields = null;
      this.indexSafeFieldName = function (field) {
        // lower case comparison and remove white spaces and other stuff
        // - this must match the corresponding method for index safe field names in the C# code!
        return field.name.replace(/[^a-z0-9-_]/gi, "");
      }
      this.registerFields = function (allFields) {
        this.fields = allFields;
      };
      this.isNamedField = function (field) {
        return field.hasOwnProperty("name");
      }
      this.validateFieldName = function (field) {
        if (this.isNamedField(field) == false) {
          // no name property - must be a field with no index name
          return true;
        }
        if (field.name == null || field.name == "") {
          return false;
        }

        var $this = this;
        var fieldWithSameName = _.find(this.fields, function (f) {
          return $this.isNamedField(f) && f != field && $this.indexSafeFieldName(f) == $this.indexSafeFieldName(field);
        });
        if (fieldWithSameName != null) {
          return false;
        }
        return true;
      };
      // added for future extensibility
      this.validateField = function (field) {
        return this.validateFieldName(field);
      };
    }
    return new propertyEditorFieldValidator();
  }
]);
