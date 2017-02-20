using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FormEditor.NewsletterSubscription
{
	public class CampaignMonitorApi
	{
		public bool Subscribe(string listId, MailAddress email, string name, string apiKey)
		{
			var emailAddress = email.Address;

			var uri = new Uri(string.Format("https://api.createsend.com/api/v3.1/subscribers/{0}.json", listId));

			var client = new WebClient();

			// set JSON content type
			client.Headers[HttpRequestHeader.ContentType] = "application/json";

			// basic auth, base64 encode of username:password
			var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", apiKey, "-")));
			client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);

			try
			{
				var data = Serialize(new SubscriptionData
				{
					EmailAddress = emailAddress,
					Name = string.IsNullOrWhiteSpace(name) ? emailAddress : name
				});

				var response = client.UploadString(uri, "POST", data);
				return emailAddress.Equals(response, StringComparison.OrdinalIgnoreCase);
			}
			catch(WebException wex)
			{
				using(var reader = new StreamReader(wex.Response.GetResponseStream()))
				{
					var response = reader.ReadToEnd();
					Log.Error(wex, string.Format("An error occurred while trying to subscribe the email: {0}. Error details: {1}", emailAddress, response));
				}
				return false;
			}
			catch(Exception ex)
			{
				Log.Error(ex, string.Format("An error occurred while trying to subscribe the email: {0}.", emailAddress));
				return false;
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
			// make sure we're serializing with the default contract resolver
			return new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() };
		}

		private class SubscriptionData
		{
			public string EmailAddress { get; set; }

			public string Name { get; set; }

			public bool Resubscribe { get { return true; } }

			public bool RestartSubscriptionBasedAutoresponders { get { return true; } }
		}
	}
}
