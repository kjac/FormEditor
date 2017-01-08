using System;
using System.Collections.Generic;
using FormEditor.Fields;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public class CustomCondition : Condition
	{
		private readonly string _type;
		private readonly string _prettyName;

		public CustomCondition(string type, string prettyName)
		{
			_type = type;
			_prettyName = prettyName;
		}

		public override string Type
		{
			get { return _type; }
		}

		public override string PrettyName
		{
			get { return _prettyName; }
		}

		public override string View
		{
			get { return @"core.customcondition"; }
		}

		public override bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content)
		{
			// custom conditions by configuration should never ever be attempted validated on the server side
			throw new NotImplementedException("IsMetBy() should never be called for custom conditions defined by configuration");
		}
	}
}
