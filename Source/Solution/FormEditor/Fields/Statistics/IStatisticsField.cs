using System.Collections.Generic;

namespace FormEditor.Fields.Statistics
{
	/// <summary>
	/// This is the base interface for a Form Editor field that supports statistics
	/// </summary>
	/// <remarks>
	/// Expect this interface to change over time as the demand for statistics grow
	/// </remarks>
	public interface IStatisticsField
	{
		/// <summary>
		/// The individual submitted values that should be put into the index for this field
		/// </summary>
		IEnumerable<string> SubmittedValues { get; }

		/// <summary>
		/// The field form safe name (should be inherited from FieldWithValue)u
		/// </summary>
		string FormSafeName { get; }

		/// <summary>
		/// The field name (should be inherited from FieldWithValue)
		/// </summary>
		string Name { get; set; }
	}
}
