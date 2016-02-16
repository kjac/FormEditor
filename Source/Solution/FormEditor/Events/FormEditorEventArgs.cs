using System;

namespace FormEditor.Events
{
	public class FormEditorEventArgs : EventArgs
	{
		public FormEditorEventArgs(Guid rowId)
		{
			RowId = rowId;
		}

		// the ID of the persisted data in the storage index
		public Guid RowId { get; private set; }
	}
}