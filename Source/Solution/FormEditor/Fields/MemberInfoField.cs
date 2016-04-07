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
			// get the logged in member (if any)
			var member = CurrentMember();
			if (member == null)
			{
				// no member logged in
				base.CollectSubmittedValue(allSubmittedValues, content);
				return;
			}
			// gather member data for index
			SubmittedValue = string.Format("{0}|{1}|{2}", member.Name, member.Email, member.Id);
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

		private string FormatValue(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			var parts = value.Split(new[] {'|'}, StringSplitOptions.None);
			return parts.Length < 2 
				? null 
				: string.Format("{0} ({1})", parts[0], parts[1]);
		}

		private IMember CurrentMember()
		{
			var user = UmbracoContext.Current.HttpContext.User;
			var identity = user != null ? user.Identity : null;
			return identity != null && string.IsNullOrWhiteSpace(identity.Name) == false
				? UmbracoContext.Current.Application.Services.MemberService.GetByUsername(identity.Name)
				: null;			
		}

		public bool IsMemberLoggedIn
		{
			get { return CurrentMember() != null; }
		}

		#region IEmailField members

		// this field implements IEmailField so receipt emails can be sent to the member email

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

		#endregion
	}
}
