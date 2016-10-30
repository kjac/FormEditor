using System;
using System.Linq;
using System.Web;
using Umbraco.Core.Models;

namespace FormEditor.Limitations
{
	public class MaxSubmissionsForCurrentUserHandler : IMaxSubmissionsForCurrentUserHandler
	{
		protected const string FormSubmittedCookieKey = "_fe";

		public virtual bool CanSubmit(FormModel model, IPublishedContent content)
		{
			if(model.DisallowMultipleSubmissionsPerUser == false)
			{
				return true;
			}
			var cookie = Request.Cookies[FormSubmittedCookieKey];
			return cookie == null || cookie.Value.Contains(FormSubmittedCookieValue(content)) == false;
		}

		public virtual void HandleSubmission(FormModel model, IPublishedContent content)
		{
			var cookieValue = (Request.Cookies.AllKeys.Contains(FormSubmittedCookieKey) ? Request.Cookies[FormSubmittedCookieKey].Value : null) ?? string.Empty;
			var containsCurrentContent = cookieValue.Contains(FormSubmittedCookieValue(content));

			if(model.DisallowMultipleSubmissionsPerUser == false)
			{
				if(containsCurrentContent)
				{
					// "only one submission per user" must've been enabled for this form at some point - explicitly remove the content ID from the cookie
					cookieValue = cookieValue.Replace(FormSubmittedCookieValue(content), ",");
					if(cookieValue == ",")
					{
						// this was the last content ID - remove the cookie 
						Response.Cookies.Add(new HttpCookie(FormSubmittedCookieKey, cookieValue) { Expires = DateTime.Today.AddDays(-1) });
					}
					else
					{
						// update the cookie value
						Response.Cookies.Add(new HttpCookie(FormSubmittedCookieKey, cookieValue) { Expires = DateTime.Today.AddDays(30) });
					}
				}

				return;
			}

			// add the content ID to the cookie value if it's not there already
			if(containsCurrentContent == false)
			{
				cookieValue = string.Format("{0}{1}", cookieValue.TrimEnd(','), FormSubmittedCookieValue(content));
			}
			Response.Cookies.Add(new HttpCookie(FormSubmittedCookieKey, cookieValue) { Expires = DateTime.Today.AddDays(30) });
		}

		protected HttpRequest Request
		{
			get { return HttpContext.Current.Request; }
		}

		protected HttpResponse Response
		{
			get { return HttpContext.Current.Response; }
		}

		protected static string FormSubmittedCookieValue(IPublishedContent content)
		{
			return string.Format(",{0},", content.Id);
		}
	}
}
