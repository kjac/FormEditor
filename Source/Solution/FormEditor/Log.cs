using System;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace FormEditor
{
	public static class Log
	{
		public static void Info(string message, params object[] args)
		{
			LogHelper.Info<FormModel>(LogMessageWithRequestedContentId(message, args));
		}

		public static void Warning(string message, params object[] args)
		{
			LogHelper.Warn<FormModel>(LogMessageWithRequestedContentId(message, args));
		}

		public static void Error(Exception ex, string message, params object[] args)
		{
			LogHelper.Error<FormModel>(LogMessageWithRequestedContentId(message, args), ex);
		}

		private static string LogMessageWithRequestedContentId(string message, params object[] args)
		{
			var id = UmbracoContext.Current != null && UmbracoContext.Current.PublishedContentRequest != null
				? UmbracoContext.Current.PublishedContentRequest.PublishedContent.Id.ToString()
				: "n/a";
			return string.Format("{0} (requested content ID: {1})", args == null || args.Length == 0 ? message : string.Format(message, args), id);
		}
	}
}