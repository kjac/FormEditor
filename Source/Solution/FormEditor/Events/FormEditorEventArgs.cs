using System;

namespace FormEditor.Events
{
	public class FormEditorEventArgs : EventArgs
	{
        public Guid RowId { get; set; }
	}
}