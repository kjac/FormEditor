using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web;
using System.Web.Http;
using FormEditor.Rendering;
using Newtonsoft.Json;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FormEditor.Api
{
	[PluginController("FormEditorApi")]
	public class PublicController : UmbracoApiController
	{
		[HttpPost]
		public HttpResponseMessage SubmitEntry()
		{
			int id;
			if (int.TryParse(HttpContext.Current.Request.Form["_id"], out id) == false)
			{
				return ValidationErrorResponse("Could not find _id in the request data");
			}

			var content = Umbraco.TypedContent(id);
			if (content == null)
			{
				return ValidationErrorResponse("Could not any content with id {0}", id);
			}

			// set the correct culture for this request
			var domain = Services.DomainService.GetAssignedDomains(content.AncestorOrSelf(1).Id, true).FirstOrDefault();
			if(domain != null && string.IsNullOrEmpty(domain.LanguageIsoCode) == false)
			{
				var culture = new CultureInfo(domain.LanguageIsoCode);
				Thread.CurrentThread.CurrentCulture = culture;
				Thread.CurrentThread.CurrentUICulture = culture;
			}

			if (Umbraco.MemberHasAccess(content.Path) == false)
			{
				return Request.CreateUserNoAccessResponse();
			}

			var property = content.ContentType.PropertyTypes.FirstOrDefault(p => p.PropertyEditorAlias == FormModel.PropertyEditorAlias);
			if (property == null)
			{
				return ValidationErrorResponse("Could not find any form property on content with id {0}", id);
			}

			FormModel formModel = null;
			try
			{
				formModel = content.GetPropertyValue<FormModel>(property.PropertyTypeAlias);
			}
			catch (Exception ex)
			{
				return ValidationErrorResponse("Could not extract the form property on content with id {0}: {1}", id, ex.Message);
			}

			var result = formModel.CollectSubmittedValues(content, false);
			if (result == false)
			{
				var errorData = new ValidationErrorData
				{
					InvalidFields = formModel.AllValueFields().Where(f => f.Invalid).ForClientSide(),
					FailedValidations = formModel.Validations.Where(v => v.Invalid).ForClientSide()
				};
				return ValidationErrorResponse(errorData);
			}

			var successData = new SubmissionSuccessData();
			if (formModel.SuccessPageId <= 0)
			{
				return SubmissionSuccessResponse(successData);
			}

			var successPage = Umbraco.TypedContent(formModel.SuccessPageId);
			if (successPage == null || !Umbraco.MemberHasAccess(successPage.Path))
			{
				return SubmissionSuccessResponse(successData);
			}

			successData.RedirectUrl = Umbraco.NiceUrl(formModel.SuccessPageId);
			successData.RedirectUrlWithDomain = Umbraco.NiceUrlWithDomain(formModel.SuccessPageId);
			return SubmissionSuccessResponse(successData);
		}

		private HttpResponseMessage SubmissionSuccessResponse(SubmissionSuccessData successData)
		{
			return Request.CreateResponse(HttpStatusCode.OK, successData, GetFormatter());
		}

		private HttpResponseMessage ValidationErrorResponse(string message, params object[] args)
		{
			return Request.CreateResponse(HttpStatusCode.BadRequest, string.Format(message, args), GetFormatter());
		}

		private HttpResponseMessage ValidationErrorResponse(ValidationErrorData errorData)
		{
			return Request.CreateResponse(HttpStatusCode.BadRequest, errorData, GetFormatter());
		}

		private static JsonMediaTypeFormatter GetFormatter()
		{
			return new JsonMediaTypeFormatter { SerializerSettings = SerializationHelper.SerializerSettings };
		}

		public class ValidationErrorData
		{
			[JsonProperty("invalidFields")]
			public List<FieldData> InvalidFields { get; set; }
			[JsonProperty("failedValidations")]
			public List<ValidationData> FailedValidations { get; set; }
		}

		public class SubmissionSuccessData
		{
			[JsonProperty("redirectUrl")]
			public string RedirectUrl { get; set; }

			[JsonProperty("redirectUrlWithDomain")]
			public string RedirectUrlWithDomain { get; set; }
		}
	}
}
