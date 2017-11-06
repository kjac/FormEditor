using System.Collections.Generic;
using System.Linq;

namespace FormEditor.Fields.Statistics
{
	public static class FieldExtensions
	{
		public static IEnumerable<IStatisticsField> StatisticsFields(this IEnumerable<Field> fields)
		{
			return fields?.OfType<IStatisticsField>().ToArray() ?? new IStatisticsField[] {};
		}

		public static IEnumerable<string> StatisticsFieldNames(this IEnumerable<IStatisticsField> fields)
		{
			return fields?.Select(f => f.FormSafeName).ToArray() ?? new string[] {};
		} 
	}
}
