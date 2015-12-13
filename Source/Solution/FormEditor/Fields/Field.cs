using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public abstract class Field
	{
		public abstract string Type { get; }

		// this is the default name for the field - used if no localized name is found
		public abstract string PrettyName { get; }

		[JsonIgnore]
		public virtual bool Invalid { get; set; }

		[JsonIgnore]
		public virtual bool CanBeAddedToForm
		{
			get
			{
				return true;
			}
		}

		public virtual string View
		{
			get
			{
				return string.Format("{0}.html", Type.ToLowerInvariant());
			}
		}

		public virtual string Icon
		{
			get
			{
				return string.Format("{0}.png", Type.ToLowerInvariant());
			}
		}

		protected internal virtual bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			return true;
		}

		protected internal virtual void CollectSubmittedValue(Dictionary<string, string> allSubmittedValues, IPublishedContent content)
		{
		}
	}
}
