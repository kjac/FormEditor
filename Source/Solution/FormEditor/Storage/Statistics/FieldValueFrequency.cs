namespace FormEditor.Storage.Statistics
{
	public class FieldValueFrequency
	{
		public FieldValueFrequency(string value, int frequency)
		{
			Value = value;
			Frequency = frequency;
		}

		public string Value { get; }

		public int Frequency { get; }
	}
}