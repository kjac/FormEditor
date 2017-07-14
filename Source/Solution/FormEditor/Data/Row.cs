using System;
using System.Collections.Generic;

namespace FormEditor.Data
{
	public class Row
	{
		public Guid Id { get; set; }
		public DateTime CreatedDate { get; set; }
		public ApprovalState ApprovalState { get; set; }
		public IEnumerable<Field> Fields { get; set; }
	}
}