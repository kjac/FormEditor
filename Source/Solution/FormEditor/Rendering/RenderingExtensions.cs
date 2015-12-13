using System.Collections.Generic;
using System.Linq;
using System.Web;
using FormEditor.Fields;
using FormEditor.Validation.Conditions;
using Newtonsoft.Json;

namespace FormEditor.Rendering
{
	public static class RenderingExtensions
	{
		public static List<ValidationData> ForClientSide(this IEnumerable<Validation.Validation> validations)
		{
			return validations.Select(v => new ValidationData
			{
				Rules = v.Rules.Select(r => new RuleData
				{
					Field = ToFieldData(r.Field),
					Condition = new ConditionData
					{
						Type = r.Condition.Type,
						ExpectedFieldValue = (r.Condition is IExpectedFieldValueCondition ? ((IExpectedFieldValueCondition)r.Condition).ExpectedFieldValue : null),
						OtherFieldName = (r.Condition is IFieldComparisonCondition ? ((IFieldComparisonCondition)r.Condition).GetOtherFieldFormSafeName() : null)
					}
				}).ToList(),
				Invalid = v.Invalid,
				ErrorMessage = v.ErrorMessage
			}).ToList();
		}

		public static IHtmlString Render(this IEnumerable<Validation.Validation> validations)
		{
			return new HtmlString(
				JsonConvert.SerializeObject(validations.ForClientSide(), SerializationHelper.SerializerSettings)
			);
		}

		public static List<FieldData> ForClientSide(this IEnumerable<FieldWithValue> fields)
		{
			return fields.Select(ToFieldData).ToList();
		}

		public static IHtmlString Render(this IEnumerable<FieldWithValue> fields)
		{
			return new HtmlString(
				JsonConvert.SerializeObject(fields.ForClientSide(), SerializationHelper.SerializerSettings)
			);
		}

		public static bool HasDefaultValue(this FieldWithValue field)
		{
			var fieldWithFieldValues = field as FieldWithFieldValues;
			return fieldWithFieldValues != null && fieldWithFieldValues.FieldValues.Any(f => f.Selected);
		}

		public static IHtmlString DefaultValue(this FieldWithValue field)
		{
			var fieldWithFieldValues = field as FieldWithFieldValues;
			if (fieldWithFieldValues == null)
			{
				return new HtmlString("undefined");
			}
			var defaultValues = fieldWithFieldValues.FieldValues.Where(f => f.Selected).ToArray();
			if (fieldWithFieldValues.IsMultiSelectEnabled)
			{
				return new HtmlString(string.Format("[{0}]", string.Join(",", defaultValues.Select(v => "\"" + HttpUtility.JavaScriptStringEncode(v.Value) + "\""))));
			}
			return new HtmlString(
				defaultValues.Any()
					? string.Format("\"{0}\"", HttpUtility.JavaScriptStringEncode(defaultValues.First().Value))
					: "undefined"
			);
		}

		private static FieldData ToFieldData(FieldWithValue f)
		{
			return new FieldData
			{
				Name = f.Name,
				FormSafeName = f.FormSafeName,
				SubmittedValue = f.SubmittedValue,
				Invalid = f.Invalid
			};
		}
	}
}
