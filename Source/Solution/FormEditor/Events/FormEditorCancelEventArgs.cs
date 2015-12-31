using System.ComponentModel;

namespace FormEditor.Events
{
	public class FormEditorCancelEventArgs : CancelEventArgs
	{
		public string ErrorMessage { get; set; }
	}
}