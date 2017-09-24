using System;
using System.Collections.Generic;

namespace FormEditor.Storage
{
	public class Row
	{
		public Row(Guid id, DateTime createdDate, Dictionary<string, string> fields, ApprovalState approvalState = ApprovalState.Undecided)
		{
			Id = id;
			Fields = fields;
			CreatedDate = createdDate;
			ApprovalState = approvalState;
		}

		public Guid Id { get; private set; }
		public DateTime CreatedDate { get; private set; }
		public ApprovalState ApprovalState { get; private set; }
		public Dictionary<string, string> Fields { get; private set; }
	}
}
