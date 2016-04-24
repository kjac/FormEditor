namespace FormEditor.Storage
{
	/// <summary>
	/// This interface describes a Form Editor storage index that supports full text search
	/// </summary>
	public interface IFullTextIndex
	{
		/// <summary>
		/// Search entries in the index
		/// </summary>
		/// <param name="searchQuery">The search query to match</param>
		/// <param name="searchFields">The fields to search</param>
		/// <param name="sortField">The entry field to sort by</param>
		/// <param name="sortDescending">True to sort the entries descending, false otherwise</param>
		/// <param name="count">The number of entries to return</param>
		/// <param name="skip">The number of entries to skip (for pagination)</param>
		/// <returns>The matching entries</returns>
		Result Search(string searchQuery, string[] searchFields, string sortField, bool sortDescending, int count, int skip);
	}
}