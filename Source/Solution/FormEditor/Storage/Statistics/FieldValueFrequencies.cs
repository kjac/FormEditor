using System.Collections.Generic;

namespace FormEditor.Storage.Statistics
{
	public class FieldValueFrequencies<T>
	{
		public FieldValueFrequencies(T field, IEnumerable<FieldValueFrequency> frequencies)
		{
			Field = field;
			Frequencies = frequencies;
		}

		public T Field { get; private set; }

		public IEnumerable<FieldValueFrequency> Frequencies { get; private set; }
	}
}