using System;
using System.Configuration;
using FormEditor.Storage;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace FormEditor.Umbraco
{
	public class ApplicationEvents : ApplicationEventHandler
	{
		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			// if the app setting "FormEditor.PreserveIndexes" is set to true, don't bother listening for delete events
			if((ConfigurationManager.AppSettings["FormEditor.PreserveIndexes"] ?? string.Empty).ToLowerInvariant() == "true")
			{
				return;
			}
			ContentService.Deleted += ContentServiceOnDeleted;
		}

		private void ContentServiceOnDeleted(IContentService sender, DeleteEventArgs<IContent> deleteEventArgs)
		{
			foreach(var deletedEntity in deleteEventArgs.DeletedEntities)
			{
				try
				{
					var formModelProperty = ContentHelper.GetFormModelProperty(deletedEntity.ContentType);
					if(formModelProperty == null)
					{
						continue;
					}
					var index = IndexHelper.GetIndex(deletedEntity.Id);
					index.Delete();
				}
				catch(Exception ex)
				{
					Log.Error(ex, "Could not delete the index for deleted content with ID: {0}", deletedEntity.Id);
				}
			}	
		}
	}
}
