using System.Collections.Generic;
using System.Linq;
using FormEditor.Fields.Statistics;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public abstract class FieldWithFieldValues : FieldWithMandatoryValidation, IValueFrequencyStatisticsField
	{
		public FieldValue[] FieldValues { get; set; }

		protected internal override void CollectSubmittedValue(Dictionary<string, string> allSubmittedValues, IPublishedContent content)
		{
			base.CollectSubmittedValue(allSubmittedValues, content);
			if(string.IsNullOrEmpty(SubmittedValue))
			{
				return;
			}
			if(SubmittedValue.StartsWith("[\"") && SubmittedValue.EndsWith("\"]"))
			{
				// #168: if the submitted value is a JSON string array, parse it to the expected CSV format 
				SubmittedValue = string.Join(",", JsonConvert.DeserializeObject<string[]>(SubmittedValue));
			}
		}

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if(base.ValidateSubmittedValue(allCollectedValues, content) == false)
			{
				return false;
			}
			if(string.IsNullOrEmpty(SubmittedValue))
			{
				// nothing selected => valid (mandatory validation is handled by base class)
				return true;
			}

			var submittedFieldValues = ExtractSubmittedValues();
			FieldValues.ToList().ForEach(f => f.Selected = submittedFieldValues.Contains(f.Value));

			// make sure all submitted values are actually defined as a field value (maybe some schmuck tampered with the options client side)
			if (submittedFieldValues.Any())
			{
				return submittedFieldValues.All(v => FieldValues.Any(f => f.Value == v));
			}

			return true;
		}

		public virtual bool IsMultiSelectEnabled => false;

		[JsonIgnore]
		public IEnumerable<string> SubmittedValues => ExtractSubmittedValues();

		private string[] ExtractSubmittedValues()
		{
			return SubmittedValue != null
				? IsMultiSelectEnabled
					? SubmittedValue.Split(',')
					: new[] {SubmittedValue}
				: new string[] {};
		}

		public virtual bool MultipleValuesPerEntry => IsMultiSelectEnabled;
	}
}