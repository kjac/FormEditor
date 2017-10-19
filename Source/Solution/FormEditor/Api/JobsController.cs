using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FormEditor.Storage;
using FormEditor.Umbraco;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FormEditor.Api
{
	[PluginController("FormEditorApi")]
	public class JobsController : UmbracoApiController
	{
		[HttpGet, HttpPost]
		public HttpResponseMessage PurgeExpiredSubmissions(string authToken)
		{
			// validate authentication token
			if (FormEditor.Configuration.Instance.Jobs == null || FormEditor.Configuration.Instance.Jobs.IsValidAuthToken(authToken) == false)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid authentication token");
			}

			try
			{
				ContentHelper.ForEachFormModel(ApplicationContext.Services, (formModel, content) => 
				{
					if (formModel.DaysBeforeSubmissionExpiry.HasValue == false || formModel.DaysBeforeSubmissionExpiry.Value <= 0)
					{
						return;
					}
					var olderThan = DateTime.UtcNow.AddDays(-1 * formModel.DaysBeforeSubmissionExpiry.Value);
					if (!(IndexHelper.GetIndex(content.Id) is IAutomationIndex index))
					{
						Log.Warning($"Unable to purge expired submissions - the configured storage index is not of type {nameof(IAutomationIndex)}");
						return;
					}
					index.RemoveOlderThan(olderThan);
				});

				return Request.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Could not purge expired submissions");
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not purge expired submissions", ex);
			}
		}
	}
}
