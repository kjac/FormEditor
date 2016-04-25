using System.Collections.Generic;

namespace FormEditor.Storage.Statistics
{
	public class FieldValueFrequencyStatistics
	{
		public int TotalRows { get; set; }

		public IDictionary<string, IEnumerable<FieldValueFrequency>> FieldValueFrequencies { get; set; }
	}
}