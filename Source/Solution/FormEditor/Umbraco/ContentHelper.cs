using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace FormEditor.Umbraco
{
	internal class ContentHelper
	{
		public static IContent GetById(int documentId)
		{
			// get the unpublished content in case the document has been unpublished after data collection
			return UmbracoContext.Current.Application.Services.ContentService.GetById(documentId);
		}

		public static FormModel GetFormModel(IContent document)
		{
			if (document == null)
			{
				return null;
			}
			var property = GetFormModelProperty(document.ContentType);
			if (property == null)
			{
				return null;
			}
			var json = document.GetValue(property.Alias) as string;
			if (string.IsNullOrEmpty(json))
			{
				return null;
			}
			return SerializationHelper.DeserializeFormModel(json);
		}

		public static PropertyType GetFormModelProperty(IContentType contentType)
		{
			var property = contentType.PropertyTypes.FirstOrDefault(p => p.PropertyEditorAlias == FormModel.PropertyEditorAlias);
			return property;
		}

		public static bool IpDisplayEnabled(IDictionary<string, PreValue> preValues)
		{
			return PreValueEnabled(preValues, "showIp");
		}

		public static bool IpLoggingEnabled(IDictionary<string, PreValue> preValues)
		{
			return PreValueEnabled(preValues, "logIp");
		}

		public static bool StatisticsEnabled(IDictionary<string, PreValue> preValues)
		{
			return PreValueEnabled(preValues, "enableStatistics");
		}

		public static bool ApprovalEnabled(IDictionary<string, PreValue> preValues)
		{
			return PreValueEnabled(preValues, "enableApproval");
		}

		public static IDictionary<string, PreValue> GetPreValues(IContent document, string propertyEditorAlias)
		{
			var property = document?.ContentType.PropertyTypes.FirstOrDefault(p => p.PropertyEditorAlias == propertyEditorAlias);
			if (property == null)
			{
				return null;
			}
			var preValues = UmbracoContext.Current.Application.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(property.DataTypeDefinitionId);
			return preValues?.PreValuesAsDictionary;
		}

		private static bool PreValueEnabled(IDictionary<string, PreValue> preValues, string preValueKey)
		{
			if (preValues == null)
			{
				return false;
			}
			return preValues.ContainsKey(preValueKey) && preValues[preValueKey] != null && (preValues[preValueKey].Value == "1");
		}
	}
}
