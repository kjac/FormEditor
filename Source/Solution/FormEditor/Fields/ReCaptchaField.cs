using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class ReCaptchaField : Field, IFieldWithValidation
	{
		private string _userResponse = null;

		public const string ResponseFieldName = "g-recaptcha-response";

		public string ErrorMessage { get; set; }

		public override string PrettyName
		{
			get { return "Spam protection (reCAPTCHA)"; }
		}

		public override string Type
		{
			get { return "core.recaptcha"; }
		}

		public virtual string Name
		{
			get { return "reCAPTCHA"; }
		}

		public virtual string FormSafeName
		{
			get { return Name; }
		}

		public override bool CanBeAddedToForm
		{
			get
			{
				return HasValidConfiguration();
			}
		}

		private bool HasValidConfiguration()
		{
			return string.IsNullOrEmpty(PublicKey) == false
			       && string.IsNullOrEmpty(PrivateKey) == false;
		}

		public string PublicKey
		{
			get
			{
				return ConfigurationManager.AppSettings["FormEditor.reCAPTCHA.SiteKey"];
			}
		}

		public string PrivateKey
		{
			get
			{
				return ConfigurationManager.AppSettings["FormEditor.reCAPTCHA.SecretKey"];
			}
		}

		private class ReCapthcaResponse
		{
			public bool Success { get; set; }
			public string[] ErrorCodes { get; set; }
		}

		protected internal override void CollectSubmittedValue(Dictionary<string, string> allSubmittedValues, IPublishedContent content)
		{
			if (allSubmittedValues.ContainsKey(ResponseFieldName) == false)
			{
				return;
			}
			if(HasValidConfiguration() == false)
			{
				Log.Warning("The reCAPTCHA configuration is invalid. Please enter the reCAPTCHA site key in the appSetting \"FormEditor.reCAPTCHA.SiteKey\" and the reCAPTCHA secret key in the appSetting \"FormEditor.reCAPTCHA.SecretKey\".");
				return;
			}
			_userResponse = allSubmittedValues[ResponseFieldName];
		}

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if (string.IsNullOrWhiteSpace(_userResponse))
			{
				return false;
			}

			var remoteIp = HttpContext.Current.Request.UserHostAddress;
			if (remoteIp == "::1")
			{
				remoteIp = "127.0.0.1";
			}

			var client = new HttpClient
			{
				BaseAddress = new Uri(@"https://www.google.com/recaptcha/api/siteverify")
			};
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = client.GetAsync(string.Format("?secret={0}&response={1}&remoteip={2}", PrivateKey, _userResponse, remoteIp)).Result;
			if (response.IsSuccessStatusCode)
			{
				var result = response.Content.ReadAsAsync<ReCapthcaResponse>().Result;
				return result.Success;
			}

			Log.Warning("reCAPTCHA field could not validate against Google - got bad response status: {0}", response.StatusCode);
			return false;
		}
	}
}
