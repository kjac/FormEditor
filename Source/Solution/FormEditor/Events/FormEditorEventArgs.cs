using System;
using Umbraco.Core.Models;

namespace FormEditor.Events
{
	public class FormEditorEventArgs : EventArgs
	{
		public FormEditorEventArgs(Guid rowId, IPublishedContent content)
		{
			RowId = rowId;
			Content = content;
		}

		// the ID of the persisted data in the storage index
		public Guid RowId { get; private set; }

		// the content that contains the form
		public IPublishedContent Content { get; private set; }
	}
}