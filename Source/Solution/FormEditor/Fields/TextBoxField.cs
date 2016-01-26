using System.Collections.Generic;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class TextBoxField : FieldWithPlaceholder
	{
		public override string PrettyName
		{
			get { return "Text box"; }
		}
		public override string Type
		{
			get { return "core.textbox"; }
		}
		public int MaxLength { get; set; }

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			if (base.ValidateSubmittedValue(allCollectedValues, content) == false)
			{
				return false;
			}
			if (MaxLength <= 0)
			{
				return true;
			}
			return string.IsNullOrEmpty(SubmittedValue) || SubmittedValue.Length <= MaxLength;
		}
	}
}
