using System;

namespace FormEditor.Storage
{
	/// <summary>
	/// This interface describes a Form Editor storage index that supports automation
	/// </summary>
	/// <remarks>
	/// Be aware that this interface will evolve continuously as the need for automation grows
	/// </remarks>
	public interface IAutomationIndex
	{
		/// <summary>
		/// Removes entries from the index older than a specified date
		/// </summary>
		/// <param name="date">The max age for entries to keep (inclusive)</param>
		void RemoveOlderThan(DateTime date);
	}
}
