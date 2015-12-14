using FormEditor.SqlIndex.Storage;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace FormEditor.SqlIndex
{
	public class ApplicationEvents : ApplicationEventHandler
	{
		protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
		{
			// automatically create the tables for the index on app start
			var db = applicationContext.DatabaseContext.Database;
			if (db.TableExist("FormEditorEntries") == false)
			{
				db.CreateTable<Entry>(false);
			}
			if (db.TableExist("FormEditorFiles") == false)
			{
				db.CreateTable<File>(false);
			}
		}
	}
}