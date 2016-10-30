using Umbraco.Core.Models;

namespace FormEditor.Limitations
{
	/// <summary>
	/// This interface describes the Form Editor handling for maximum submissions for the currently active user
	/// </summary>
	public interface IMaxSubmissionsForCurrentUserHandler
	{
		/// <summary>
		/// Whether or not the current user can submit a form
		/// </summary>
		/// <param name="model">The form</param>
		/// <param name="content">The content that contains the form</param>
		/// <returns>True if the current user can submit the form, false otherwise</returns>
		bool CanSubmit(FormModel model, IPublishedContent content);

		/// <summary>
		/// Performs any handling after the current user has submitted the form - e.g. register the user as having submitted the form
		/// </summary>
		/// <param name="model">The form</param>
		/// <param name="content">The content that contains the form</param>
		void HandleSubmission(FormModel model, IPublishedContent content);
	}
}
