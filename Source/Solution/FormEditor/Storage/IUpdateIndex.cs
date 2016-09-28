using System;
using System.Collections.Generic;

namespace FormEditor.Storage
{
	/// <summary>
	/// This interface describes a Form Editor storage index that supports updating entries
	/// </summary>
	public interface IUpdateIndex : IIndex
	{
		/// <summary>
		/// Updates an entry in the index
		/// </summary>
		/// <param name="fields">The field names and values to update</param>
		/// <param name="rowId">The ID of the entry to update</param>
		/// <returns>The ID of the form entry</returns>
		Guid Update(Dictionary<string, string> fields, Guid rowId);

		/// <summary>
		/// Updates an entry in the index
		/// </summary>
		/// <param name="fields">The field names and values to update</param>
		/// <param name="fieldsValuesForStatistics">The field names and values to create statistics for</param>
		/// <param name="rowId">The ID of the entry to update</param>
		/// <returns>The ID of the form entry</returns>
		Guid Update(Dictionary<string, string> fields, Dictionary<string, IEnumerable<string>> fieldsValuesForStatistics, Guid rowId);
	}
}