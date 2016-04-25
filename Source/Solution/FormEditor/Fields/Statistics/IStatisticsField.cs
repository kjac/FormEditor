using System.Collections.Generic;

namespace FormEditor.Fields.Statistics
{
	/// <summary>
	/// This interface describes a Form Editor field that supports statistics
	/// </summary>
	/// <remarks>
	/// Expect this interface to change over time as the demand for statistics grow
	/// </remarks>
	public interface IStatisticsField
	{
		// TODO: document this
		IEnumerable<string> SubmittedValues { get; }

		// TODO: document this
		string FormSafeName { get; }

		string Name { get; set; }
	}
}
