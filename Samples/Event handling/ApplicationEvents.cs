using System;
using System.Linq;
using FormEditor;
using FormEditor.Events;
using Umbraco.Core;

namespace My.Events
{
	public class ApplicationEvents : ApplicationEventHandler
	{
		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			FormModel.BeforeAddToIndex += FormModelOnBeforeAddToIndex;
			FormModel.AfterAddToIndex += FormModelOnAfterAddToIndex;
		}

		private void FormModelOnBeforeAddToIndex(FormModel sender, FormEditorCancelEventArgs formEditorCancelEventArgs)
		{
			if(sender.AllValueFields().Any(f => f.HasSubmittedValue && f.SubmittedValue.Equals("bad", StringComparison.InvariantCultureIgnoreCase)))
			{
				formEditorCancelEventArgs.Cancel = true;
				// you can supply multiple error messages by using the FormEditorCancelEventArgs.ErrorMessages array,
				// or if you only have one message message, you can simply use the FormEditorCancelEventArgs.ErrorMessage property
				//formEditorCancelEventArgs.ErrorMessage ="Bad values are not accepted.";
				formEditorCancelEventArgs.ErrorMessages = new[] {"Bad values are not accepted.", "Even worse ones aren't either."};
			}
		}

		private void FormModelOnAfterAddToIndex(FormModel sender, FormEditorEventArgs formEditorEventArgs)
		{
			Log.Info("Something was added to the index.");
		}
	}
}