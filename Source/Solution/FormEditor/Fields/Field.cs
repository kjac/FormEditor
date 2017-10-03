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
		public virtual bool CanBeAddedToForm => true;

		public virtual string View => $"{Type.ToLowerInvariant()}.html";

		public virtual string Icon => $"{Type.ToLowerInvariant()}.png";

		protected internal virtual bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			return true;
		}

		protected internal virtual void CollectSubmittedValue(Dictionary<string, string> allSubmittedValues, IPublishedContent content)
		{
		}

		// this is called after a form submission has successfully been added to the index
		protected internal virtual void AfterAddToIndex(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
		}
	}
}
