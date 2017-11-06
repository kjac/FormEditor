using System;
using System.Collections.Generic;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class CustomCondition : Condition
	{
		public CustomCondition(string type, string prettyName)
		{
			Type = type;
			PrettyName = prettyName;
		}

		public override string Type { get; }

		public override string PrettyName { get; }

		public override string View => @"core.customcondition";

		public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			// custom conditions by configuration should never ever be attempted validated on the server side
			throw new NotImplementedException("IsMetBy() should never be called for custom conditions defined by configuration");
		}
	}
}
