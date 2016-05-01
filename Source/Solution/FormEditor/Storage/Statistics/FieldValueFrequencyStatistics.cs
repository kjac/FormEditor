using System.Collections.Generic;

namespace FormEditor.Storage.Statistics
{
	public class FieldValueFrequencyStatistics<T>
	{
		private readonly List<FieldValueFrequencies<T>> _fieldValueFrequencies;

		public FieldValueFrequencyStatistics(int totalRows)
		{
			TotalRows = totalRows;
			_fieldValueFrequencies = new List<FieldValueFrequencies<T>>();
		}

		public void Add(T field, IEnumerable<FieldValueFrequency> fieldValueFrequencies)
		{
			_fieldValueFrequencies.Add(new FieldValueFrequencies<T>(field, fieldValueFrequencies));
		}

		public int TotalRows { get; private set; }

		public IEnumerable<FieldValueFrequencies<T>> FieldValueFrequencies
		{
			get { return _fieldValueFrequencies; }
		}
	}
}