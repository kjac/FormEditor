using System.Collections.Generic;

namespace FormEditor.Storage.Statistics
{
	public class FieldValueFrequencyStatistics
	{
		private readonly IDictionary<string, IEnumerable<FieldValueFrequency>> _fieldValueFrequencies;

		public FieldValueFrequencyStatistics()
		{
			_fieldValueFrequencies = new Dictionary<string, IEnumerable<FieldValueFrequency>>();
		}

		public void Add(string fieldName, IEnumerable<FieldValueFrequency> fieldValueFrequencies)
		{
			_fieldValueFrequencies[fieldName] = fieldValueFrequencies;
		}

		public int TotalRows { get; set; }

		public IEnumerable<KeyValuePair<string, IEnumerable<FieldValueFrequency>>> FieldValueFrequencies
		{
			get { return _fieldValueFrequencies; }
		}
	}
}