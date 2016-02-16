using System.Collections.Generic;
using System.ComponentModel;
using FormEditor.Fields;

namespace FormEditor.Events
{
	public class FormEditorCancelEventArgs : CancelEventArgs
	{
		// if Cancel is set to true, use this property to describe the error to the user
		public string ErrorMessage { get; set; }
	}
}