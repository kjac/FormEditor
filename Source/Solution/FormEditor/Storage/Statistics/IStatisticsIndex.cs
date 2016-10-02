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
		/// <param name="fieldsValuesForStatistics">The field names and values to create statistics for</param>
		/// <param name="rowId">The ID of the entry to add</param>
		/// <returns>The ID of the form entry</returns>
		Guid Add(Dictionary<string, string> fields, Dictionary<string, IEnumerable<string>> fieldsValuesForStatistics, Guid rowId);

		/// <summary>
		/// Updates an entry to the index
		/// </summary>
		/// <param name="fields">The field names and values to update</param>
		/// <param name="fieldsValuesForStatistics">The field names and values to create statistics for</param>
		/// <param name="rowId">The ID of the entry to update</param>
		/// <returns>The ID of the form entry</returns>
		Guid Update(Dictionary<string, string> fields, Dictionary<string, IEnumerable<string>> fieldsValuesForStatistics, Guid rowId);

		/// <summary>
		/// Gets the field value frequency statistics for specified fields
		/// </summary>
		/// <param name="fieldNames">The field names to get statistics for</param>
		/// <returns>The field value frequencies</returns>
		FieldValueFrequencyStatistics<string> GetFieldValueFrequencyStatistics(IEnumerable<string> fieldNames);
	}
}
