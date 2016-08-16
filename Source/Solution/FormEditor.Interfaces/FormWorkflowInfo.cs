using System;
using System.Collections.Generic;

namespace FormEditor.Interfaces
{
    public class FormWorkflowInfo
    {
      
            private Type x;

            public FormWorkflowInfo(Type x)
            {
                FormEditorWorkflow workflow = (FormEditorWorkflow)Activator.CreateInstance(x);
                this.Name = workflow.Name;
                this.Description = workflow.Description;
                this.Properties = workflow.Properties;
                this.WokflowType = x.FullName;
                
            }

            public string Description { get;  set; }
            public string Name { get;  set; }
            public IEnumerable<FormEditorProperties> Properties { get;  set; }
            public string WokflowType { get; set; }
    }
}