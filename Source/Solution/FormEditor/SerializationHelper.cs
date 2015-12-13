using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FormEditor
{
	public static class SerializationHelper
	{
		public static IContractResolver ContractResolver
		{
			get { return new CamelCasePropertyNamesContractResolver(); }
		}

		public static TypeNameHandling TypeNameHandling
		{
			get { return TypeNameHandling.Auto; }
		}

		public static FormModel DeserializeFormModel(string json)
		{
			// see PropertyEditorController for an explanation :)
			json = json.ToString().Replace(@"""runtimeType""", @"""$type""");

			return JsonConvert.DeserializeObject<FormModel>(json, SerializerSettings);
		}

		public static JsonSerializerSettings SerializerSettings
		{
			get
			{
				return new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling,
					ContractResolver = ContractResolver
				};
			}
		}
	}
}
