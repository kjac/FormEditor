using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace FormEditor
{
	[PropertyValueType(typeof(FormModel))]
	[PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
	public class FormValueConverter : PropertyValueConverterBase
	{
		public override bool IsConverter(PublishedPropertyType propertyType)
		{
			return propertyType.PropertyEditorAlias.Equals(FormModel.PropertyEditorAlias);
		}

		public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
		{
			if (string.IsNullOrWhiteSpace(source?.ToString()))
			{
				return new FormModel();
			}

			var model = SerializationHelper.DeserializeFormModel(source.ToString());
			return model;
		}
	}
}
