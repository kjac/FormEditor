using System;

namespace FormEditor.Storage
{
	/// <summary>
	/// This interface describes a Form Editor storage index that supports approval per form entry
	/// </summary>
	public interface IApprovalIndex
	{
		/// <summary>
		/// Gets entries from the index
		/// </summary>
		/// <param name="sortField">The entry field to sort by</param>
		/// <param name="sortDescending">True to sort the entries descending, false otherwise</param>
		/// <param name="count">The number of entries to return</param>
		/// <param name="skip">The number of entries to skip (for pagination)</param>
		/// <param name="approvalState">The approval state of the entries - if the value is ApprovalState.Any, no approval filtering is applied</param>
		/// <returns>The matching entries</returns>
		Result Get(string sortField, bool sortDescending, int count, int skip, ApprovalState approvalState);

		/// <summary>
		/// Sets the approval state of an entry in the index
		/// </summary>
		/// <param name="approvalState">The new approval state for the entry</param>
		/// <param name="rowId">The ID of the entry to update</param>
		/// <returns>The ID of the form entry</returns>
		bool SetApprovalState(ApprovalState approvalState, Guid rowId);
	}
}