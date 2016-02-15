using System;

namespace FormEditor.Events
{
	public class FormEditorEventArgs : EventArgs
	{
        public Guid RawId { get; set; }
	}
}