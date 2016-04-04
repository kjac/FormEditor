using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace FormEditor.Fields
{
	public class MemberInfoField : FieldWithValue, IEmailField
	{
		public override string Type
		{
			get { return "core.memberinfo"; }
		}

		public override string PrettyName
		{
			get { return "Member info"; }
		}

		protected internal override void CollectSubmittedValue(Dictionary<string, string> allSubmittedValues, IPublishedContent content)
		{
			var currentUser = UmbracoContext.Current.Security.CurrentUser;
			if (currentUser == null)
			{
				base.CollectSubmittedValue(allSubmittedValues, content);
				return;
			}
			SubmittedValue = string.Format("{0}|{1}|{2}", currentUser.Name, currentUser.Email, currentUser.Id);
		}

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			return FormatValue(value) ?? base.FormatValueForDataView(value, content, rowId);
		}

		protected internal override string FormatValueForCsvExport(string value, IContent content, Guid rowId)
		{
			return FormatValue(value) ?? base.FormatValueForCsvExport(value, content, rowId);
		}

		protected internal override string FormatValueForFrontend(string value, IPublishedContent content, Guid rowId)
		{
			return FormatValue(value) ?? base.FormatValueForFrontend(value, content, rowId);
		}

		public IEnumerable<string> EmailAddresses
		{
			get
			{
				if (string.IsNullOrEmpty(SubmittedValue))
				{
					return null;
				}
				var parts = SubmittedValue.Split(new[] {'|'}, StringSplitOptions.None);
				return parts.Length < 2 
					? null 
					: new[] {parts[1]};
			}
		}

		private string FormatValue(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			var parts = SubmittedValue.Split(new[] {'|'}, StringSplitOptions.None);
			return parts.Length < 2 
				? null 
				: string.Format("{0} ({1})", parts[0], parts[1]);
		}
	}
}
