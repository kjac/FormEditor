using System;
using System.Configuration;
using System.Linq;
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
			// it would be ideal if ContentService.Deleted were executed per item when emptying the bin, but it's not...
			// we also need to handle this before the bin is actually emptied, because the deleted entitites can't be 
			// accessed after the bin has been emptied. so we'll clean up before it's emptied and hope the empty event 
			// is not cancelled by someone else.
			ContentService.EmptyingRecycleBin += ContentServiceOnEmptyingRecycleBin;
		}

		private void ContentServiceOnEmptyingRecycleBin(IContentService sender, RecycleBinEventArgs recycleBinEventArgs)
		{
			if(recycleBinEventArgs.IsContentRecycleBin == false)
			{
				return;
			}
			var deletedEntities = sender.GetByIds(recycleBinEventArgs.Ids).ToList();
			foreach(var deletedEntity in deletedEntities)
			{
				DeleteEntityIndex(deletedEntity);
			}
		}

		private void ContentServiceOnDeleted(IContentService sender, DeleteEventArgs<IContent> deleteEventArgs)
		{
			foreach(var deletedEntity in deleteEventArgs.DeletedEntities)
			{
				DeleteEntityIndex(deletedEntity);
			}	
		}

		private static void DeleteEntityIndex(IContent deletedEntity)
		{
			try
			{
				var formModelProperty = ContentHelper.GetFormModelProperty(deletedEntity.ContentType);
				if(formModelProperty == null)
				{
					return;
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
