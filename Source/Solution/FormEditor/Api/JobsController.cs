using System;
using System.Linq;
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
		[HttpGet]
		public HttpResponseMessage PurgeExpiredSubmissions(string authToken)
		{
			// validate authentication token
			if (FormEditor.Configuration.Instance.Jobs.IsValidAuthToken(authToken) == false)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid authentication token");
			}

			try
			{
				var contentTypes = ApplicationContext.Services.ContentTypeService.GetAllContentTypes();
				contentTypes = contentTypes.Where(c => ContentHelper.GetFormModelProperty(c) != null).ToArray();

				foreach (var contentType in contentTypes)
				{
					var contentOfContentType = ApplicationContext.Services.ContentService.GetContentOfContentType(contentType.Id).ToArray();
					foreach (var content in contentOfContentType)
					{
						var formModel = ContentHelper.GetFormModel(content);
						if (formModel == null || formModel.DaysBeforeSubmissionExpiry.HasValue == false || formModel.DaysBeforeSubmissionExpiry.Value <= 0)
						{
							continue;
						}
						var olderThan = DateTime.UtcNow.AddDays(-1 * formModel.DaysBeforeSubmissionExpiry.Value);
						var index = IndexHelper.GetIndex(content.Id);
						index.RemoveOlderThan(olderThan);
					}
				}
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
