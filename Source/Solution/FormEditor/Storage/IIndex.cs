using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace FormEditor.Storage
{
	/// <summary>
	/// This interface describes the Form Editor storage index
	/// </summary>
	public interface IIndex
	{
		/// <summary>
		/// Adds an entry to the index
		/// </summary>
		/// <param name="fields">The field names and values to add</param>
		/// <returns>The ID of the form entry</returns>
		Guid Add(Dictionary<string, string> fields);

		/// <summary>
		/// Removes entries from the index
		/// </summary>
		/// <param name="rowIds">The IDs of the entries to remove</param>
		void Remove(IEnumerable<Guid> rowIds);

		/// <summary>
		/// Gets a specific entry from the index
		/// </summary>
		/// <param name="rowId">The ID of the entry to get</param>
		/// <returns>The entry</returns>
		Row Get(Guid rowId);

		/// <summary>
		/// Gets a specific set of entries from the index
		/// </summary>
		/// <param name="rowIds">The IDs of the entries to get</param>
		/// <returns>The entries</returns>
		IEnumerable<Row> Get(IEnumerable<Guid> rowIds);

		/// <summary>
		/// Gets entries from the index
		/// </summary>
		/// <param name="sortField">The entry field to sort by</param>
		/// <param name="sortDescending">True to sort the entries descending, false otherwise</param>
		/// <param name="count">The number of entries to return</param>
		/// <param name="skip">The number of entries to skip (for pagination)</param>
		/// <returns>The matching entries</returns>
		Result Get(string sortField, bool sortDescending, int count, int skip);

		/// <summary>
		/// Saves an uploaded file to the index
		/// </summary>
		/// <param name="file">The uploaded file to save</param>
		/// <param name="filename">The filename that will be used to get the file from the index</param>
		/// <param name="rowId">The ID of the form submission entry this file is a part of</param>
		/// <returns>True if the file was saved successfully, false otherwise</returns>
		bool SaveFile(HttpPostedFile file, string filename, Guid rowId);

		/// <summary>
		/// Gets a file from the index
		/// </summary>
		/// <param name="filename">The filename used when the file was originally persisted</param>
		/// <param name="rowId">The ID of the form submission entry this file is a part of</param>
		/// <returns>The file contents as a stream</returns>
		Stream GetFile(string filename, Guid rowId);

		/// <summary>
		/// Deletes the entire index
		/// </summary>
		void Delete();
	}
}