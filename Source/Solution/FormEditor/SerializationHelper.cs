using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FormEditor
{
	public static class SerializationHelper
	{
		public static IContractResolver ContractResolver => new CamelCasePropertyNamesContractResolver();

		public static TypeNameHandling TypeNameHandling => TypeNameHandling.Auto;

		public static FormModel DeserializeFormModel(string json)
		{
			// see below for an explanation :)
			json = json.Replace(@"""runtimeType""", @"""$type""");

			return JsonConvert.DeserializeObject<FormModel>(json, SerializerSettings);
		}

		public static string SerializeFormModel(FormModel formModel)
		{
			return formModel == null 
				? null 
				: FormatJson(JsonConvert.SerializeObject(formModel, SerializerSettings));
		}

		public static JsonSerializerSettings SerializerSettings => new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling,
			ContractResolver = ContractResolver
		};

		internal static string FormatJson(string json)
		{
			// AngularJS messes with properties that start with $, so we need to swap $type with something else
			json = json.Replace(@"""$type""", @"""runtimeType""");
			return json;
		}
	}
}
