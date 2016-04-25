using System;
using System.Collections.Generic;

namespace FormEditor.Storage.Statistics
{
	/// <summary>
	/// This interface describes a Form Editor storage index that supports statistics
	/// </summary>
	/// <remarks>
	/// Expect this interface to change over time as the demand for statistics grow
	/// </remarks>
	public interface IStatisticsIndex
	{
		/// <summary>
		/// Adds an entry to the index
		/// </summary>
		/// <param name="fields">The field names and values to add</param>
		/// <param name="fieldsForStatistics">The field names and values to create statistics for</param>
		/// <param name="rowId">The ID of the entry to add</param>
		/// <returns>The ID of the form entry</returns>
		Guid Add(Dictionary<string, string> fields, Dictionary<string, IEnumerable<string>> fieldsForStatistics, Guid rowId);

		// TODO: document this
		FieldValueFrequencyStatistics GetFieldValueFrequencyStatistics(IEnumerable<string> fieldNames);
	}
}
