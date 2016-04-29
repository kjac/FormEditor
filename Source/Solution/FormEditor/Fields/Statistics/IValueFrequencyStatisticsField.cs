namespace FormEditor.Fields.Statistics
{
	/// <summary>
	/// This interface describes a Form Editor field that supports field value statistics
	/// </summary>
	/// <remarks>
	/// Expect this interface to change over time as the demand for statistics grow
	/// </remarks>
	public interface IValueFrequencyStatisticsField : IStatisticsField
	{
		/// <summary>
		/// Whether or not field type can contain multiple values per entry
		/// </summary>
		bool MultipleValuesPerEntry { get; }
	}
}