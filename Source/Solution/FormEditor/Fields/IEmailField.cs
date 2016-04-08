using System.Collections.Generic;

namespace FormEditor.Fields
{
	public interface IEmailField
	{
		IEnumerable<string> EmailAddresses { get; }
	}
}
