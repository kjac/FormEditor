using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FormEditor.NewsletterSubscription
{
	public class MailChimpApi
	{
		public bool Subscribe(string listId, string email, string apiKey)
		{
			var apiKeyParts = apiKey.Split('-');
			if(apiKeyParts.Length != 2)
			{
				Log.Warning("The MailChimp API key is invalid; it should contain the MailChimp data center (e.g. 6292043b52144da2a5ccba0ed8dd9016-us15)");
				return false;
			}

			var key = apiKeyParts.First();
			var dc = apiKeyParts.Last();
			var hash = GetMd5Hash(email.ToLowerInvariant());
			var uri = new Uri(string.Format("https://{0}.api.mailchimp.com/3.0/lists/{1}/members/{2}", dc, listId, hash));

			var client = new WebClient();

			// set JSON content type
			client.Headers[HttpRequestHeader.ContentType] = "application/json";

			// basic auth, base64 encode of username:password
			var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "-", key)));
			client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);

			try
			{
				var data = Serialize(new SubscriptionData
				{
					Email_Address = email,
					Status = "subscribed"
				});

				var response = client.UploadString(uri, "PUT", data);
				var result = Deserialize<SubscriptionData>(response);
				return "subscribed".Equals(result.Status, StringComparison.OrdinalIgnoreCase) && email.Equals(result.Email_Address, StringComparison.OrdinalIgnoreCase);
			}
			catch(WebException wex)
			{
				using(var reader = new StreamReader(wex.Response.GetResponseStream()))
				{
					var response = reader.ReadToEnd();
					Log.Error(wex, string.Format("An error occurred while trying to subscribe the email: {0}. Error details: {1}", email, response));
				}
				return false;
			}
			catch(Exception ex)
			{
				Log.Error(ex, string.Format("An error occurred while trying to subscribe the email: {0}.", email));
				return false;
			}
		}

		private static string GetMd5Hash(string input)
		{
			using(var md5Hash = MD5.Create())
			{
				var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
				var builder = new StringBuilder();
				foreach(var t in data)
				{
					builder.Append(t.ToString("x2"));
				}
				return builder.ToString();
			}
		}

		private string Serialize<T>(T data)
		{
			return JsonConvert.SerializeObject(data, SerializerSettings());
		}

		private T Deserialize<T>(string data)
		{
			return JsonConvert.DeserializeObject<T>(data, SerializerSettings());
		}

		private static JsonSerializerSettings SerializerSettings()
		{
			// make sure we're serializing with camel casing
			return new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
		}

		private class SubscriptionData
		{
			public string Email_Address { get; set; }

			public string Status { get; set; }
		}
	}
}
