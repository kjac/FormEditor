using System;

namespace FormEditor.Interfaces
{
    public class FormEditorProperties
    {
        public string PropertyAlias { get; set; }
        public string PropertyName { get; set; }
        private Type _TypedPropertyType;
        public string TypedPropertyType
        {
            get
            {
                return _TypedPropertyType.FullName;
            }
            set
            {
                // Validate Type
                _TypedPropertyType = Type.GetType(value, true, false);
            }
        }
        public object Value { get; set; }
    }
}