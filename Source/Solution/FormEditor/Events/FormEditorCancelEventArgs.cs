using System.ComponentModel;
using System.Linq;
using Umbraco.Core.Models;

namespace FormEditor.Events
{
	public class FormEditorCancelEventArgs : CancelEventArgs
	{
		public FormEditorCancelEventArgs(IPublishedContent content)
		{
			Content = content;
		}

		// if Cancel is set to true, use this property to describe the error to the user - or use the ErrorMessages property if you have multiple errors
		public string ErrorMessage
		{
			// #162: the getter is kept solely for backwards compatibility
			get => ErrorMessages?.FirstOrDefault();
			set => ErrorMessages = new[] {value};
		}

		public string[] ErrorMessages { get; set; }

		// the content that contains the form
		public IPublishedContent Content { get; }
	}
}