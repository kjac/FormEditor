using System.Collections.Generic;
using FormEditor.Fields;
using FormEditor.Rendering;
using Umbraco.Core.Models;

namespace FormEditor.Validation.Conditions
{
	public abstract class Condition
	{
		public abstract string Type { get; }
		public abstract string PrettyName { get; }
		public abstract bool IsMetBy(FieldWithValue fieldValue, IEnumerable<FieldWithValue> allCollectedFieldValues, IPublishedContent content);

		public virtual string Icon
		{
			get
			{
				return DefaultIcon(Type);
			}
		}

		public virtual ConditionData ForFrontEnd()
		{
			return new ConditionData(this);
		}

		protected static string DefaultIcon(string type)
		{
			return string.Format("{0}.png", type.ToLowerInvariant());
		}

		public virtual string View
		{
			get { return Type; }
		}
	}
}