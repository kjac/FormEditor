using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FormEditor.NewsletterSubscription
{
	public class CampaignMonitorApi
	{
		public bool Subscribe(string listId, MailAddress email, string name, Dictionary<string, string> customFields, string apiKey)
		{
			var emailAddress = email.Address;

			var uri = new Uri($"https://api.createsend.com/api/v3.1/subscribers/{listId}.json");

			var client = new WebClient
			{
				Encoding = Encoding.UTF8
			};

			// set JSON content type
			client.Headers[HttpRequestHeader.ContentType] = "application/json";

			// basic auth, base64 encode of username:password
			var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:-"));
			client.Headers[HttpRequestHeader.Authorization] = $"Basic {credentials}";

			var customFieldsData = customFields == null
				? new CustomField[0]
				: customFields.Select(kvp => new CustomField
				{
					Key = kvp.Key,
					Value = kvp.Value
				}).ToArray();

			try
			{
				var data = Serialize(new SubscriptionData
				{
					EmailAddress = emailAddress,
					Name = string.IsNullOrWhiteSpace(name) ? emailAddress : name,
					CustomFields = customFieldsData
				});

				var response = client.UploadString(uri, "POST", data);
				return emailAddress.Equals(response, StringComparison.OrdinalIgnoreCase);
			}
			catch(WebException wex)
			{
				var responseStream = wex.Response?.GetResponseStream();
				if(responseStream != null)
				{
					using(var reader = new StreamReader(responseStream))
					{
						var response = reader.ReadToEnd();
						Log.Error(wex, $"An error occurred while trying to subscribe the email: {emailAddress}. Error details: {response}");
					}
				}
				return false;
			}
			catch(Exception ex)
			{
				Log.Error(ex, $"An error occurred while trying to subscribe the email: {emailAddress}.");
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

			public CustomField[] CustomFields { get; set; }

			public bool Resubscribe => true;

			public bool RestartSubscriptionBasedAutoresponders => true;
		}

		public class CustomField
		{
			public string Key { get; set; }

			public string Value { get; set; }
		}
	}
}
