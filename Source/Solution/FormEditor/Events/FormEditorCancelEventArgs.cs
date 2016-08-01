using System.ComponentModel;
using Umbraco.Core.Models;

namespace FormEditor.Events
{
	public class FormEditorCancelEventArgs : CancelEventArgs
	{
		public FormEditorCancelEventArgs(IPublishedContent content)
		{
			Content = content;
		}

		// if Cancel is set to true, use this property to describe the error to the user
		public string ErrorMessage { get; set; }

		// the content that contains the form
		public IPublishedContent Content { get; private set; }
	}
}