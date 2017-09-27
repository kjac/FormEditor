using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace FormEditor
{
	internal static class AntiForgeryHelper
	{
		public static void ValidateAntiForgery(string tokenValue)
		{
			var cookieToken = "";
			var formToken = "";
			if(string.IsNullOrEmpty(tokenValue) == false)
			{
				var tokens = tokenValue.Split(':');
				if(tokens.Length == 2)
				{
					cookieToken = tokens[0].Trim();
					formToken = tokens[1].Trim();
				}
			}
			AntiForgery.Validate(cookieToken, formToken);
		}
	}
}
