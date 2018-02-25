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
			Index.EnsureDatabase(applicationContext);
		}

		private static bool SupportsMax(DatabaseContext dbContext)
		{
			return dbContext.DatabaseProvider == DatabaseProviders.SqlAzure || dbContext.DatabaseProvider == DatabaseProviders.SqlServer;
		}
	}
}