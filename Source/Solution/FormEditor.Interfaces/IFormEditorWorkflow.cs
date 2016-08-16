using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FormEditor.Interfaces
{
    public abstract class  FormEditorWorkflow
    {
        public string Name { get; }
        public string Description { get; }
        public IEnumerable<FormEditorProperties> Properties { get;set;}
        public void SetWorkflowProperties(IEnumerable<FormEditorProperties> properties)
        {

            foreach (FormEditorProperties p in properties)
            {
                var pMy = Properties.FirstOrDefault(x => x.PropertyAlias.Equals(p.PropertyAlias));
                if (null != pMy)
                {
                    pMy.Value = p.Value;
                }
                   
            }

        }
        public abstract void Execute(Dictionary<string, object> data, Umbraco.Core.Models.IPublishedContent contentNode, IFormModel form);
    }
}
