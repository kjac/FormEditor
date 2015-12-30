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

		public static bool IpDisplayEnabled(IContent document)
		{
			return PreValueEnabled(document, FormDataModel.PropertyEditorAlias, "showIp");
		}

		public static bool IpLoggingEnabled(IContent document)
		{
			return PreValueEnabled(document, FormModel.PropertyEditorAlias, "logIp");
		}

		private static bool PreValueEnabled(IContent document, string propertyEditorAlias, string preValueKey)
		{
			if (document == null)
			{
				return false;
			}
			var property = document.ContentType.PropertyTypes.FirstOrDefault(p => p.PropertyEditorAlias == propertyEditorAlias);
			if (property == null)
			{
				return false;
			}
			var preValues = UmbracoContext.Current.Application.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(property.DataTypeDefinitionId);
			if (preValues == null)
			{
				return false;
			}
			var preValueDictionary = preValues.PreValuesAsDictionary;
			return preValueDictionary.ContainsKey(preValueKey) && preValueDictionary[preValueKey] != null && (preValueDictionary[preValueKey].Value == "1");
		}
	}
}
