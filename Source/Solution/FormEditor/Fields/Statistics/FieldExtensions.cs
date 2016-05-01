using System.Collections.Generic;
using System.Linq;

namespace FormEditor.Fields.Statistics
{
	public static class FieldExtensions
	{
		public static IEnumerable<IStatisticsField> StatisticsFields(this IEnumerable<Field> fields)
		{
			return fields == null 
				? new IStatisticsField[] {} 
				: fields.OfType<IStatisticsField>().ToArray();
		}

		public static IEnumerable<string> StatisticsFieldNames(this IEnumerable<IStatisticsField> fields)
		{
			return fields == null 
				? new string[] {}
				: fields.Select(f => f.FormSafeName).ToArray();
		} 
	}
}
