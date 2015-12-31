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
				formEditorCancelEventArgs.ErrorMessage = "Bad values are not accepted.";
			}
		}

		private void FormModelOnAfterAddToIndex(FormModel sender, FormEditorEventArgs formEditorEventArgs)
		{
			Log.Info("Something was added to the index.");
		}
	}
}