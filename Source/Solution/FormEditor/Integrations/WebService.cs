using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Core.Models;

namespace FormEditor.Integrations
{
	public class WebService
	{
		private readonly WebServiceConfiguration _configuration;

		public WebService(WebServiceConfiguration configuration)
		{
			_configuration = configuration;
		}

		public bool Submit(FormModel formModel, IPublishedContent content)
		{
			if(_configuration.Url == null)
			{
				Log.Warning("Could not submit form data to the web service - the web service endpoint URL was not configured correctly.");
				return false;
			}

			var client = new WebClient
			{
				Encoding = Encoding.UTF8
			};

			// set content type
			client.Headers[HttpRequestHeader.ContentType] = "application/json";

			if(string.IsNullOrEmpty(_configuration.UserName) == false && string.IsNullOrEmpty(_configuration.Password) == false)
			{
				// basic auth, base64 encode of username:password
				var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", _configuration.UserName, _configuration.Password)));
				client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
			}

			try
			{
				var webServiceData = new WebServiceData
				{
					UmbracoContentName = content.Name,
					UmbracoContentId = content.Id,
					IndexRowId = formModel.RowId.ToString(),
					FormData = formModel.AllValueFields().Select(field => new FormFieldData
					{
						Name = field.Name,
						FormSafeName = field.FormSafeName,
						Type = field.Type,
						SubmittedValue = field.SubmittedValue
					}).ToArray(),
					SubmittedValues = formModel.AllValueFields().ToDictionary(
							field => field.FormSafeName,
							field => field.SubmittedValue
						)
				};

				// serialize to JSON using camel casing
				var data = JsonConvert.SerializeObject(webServiceData, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

				// set content type
				client.Headers[HttpRequestHeader.ContentType] = "application/json";

				// POST the JSON and log the response
				var response = client.UploadString(_configuration.Url, "POST", data);
				Log.Info(string.Format("Form data was submitted to endpoint: {0} - response received was: {1}", _configuration.Url, string.IsNullOrEmpty(response) ? "(none)" : response));

				return true;
			}
			catch(WebException wex)
			{
				using(var reader = new StreamReader(wex.Response.GetResponseStream()))
				{
					var response = reader.ReadToEnd();
					Log.Error(wex, string.Format("An error occurred while trying to submit form data to: {0}. Error details: {1}", _configuration.Url, response), null);
				}
				return false;
			}
			catch(Exception ex)
			{
				Log.Error(ex, string.Format("An error occurred while trying to submit form data to: {0}.", _configuration.Url));
				return false;
			}

		}

		// added these classes so they can be used for deserialization on the recipient side :)
		public class WebServiceData
		{
			public string UmbracoContentName { get; set; }

			public int UmbracoContentId { get; set; }

			public string IndexRowId { get; set; }

			public FormFieldData[] FormData { get; set; }

			public Dictionary<string, string> SubmittedValues { get; set; }
		}

		public class FormFieldData
		{
			public string Name { get; set; }

			public string FormSafeName { get; set; }

			public string Type { get; set; }

			public string SubmittedValue { get; set; }
		}
	}
}
