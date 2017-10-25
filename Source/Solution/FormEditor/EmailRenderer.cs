using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace FormEditor
{
	public class EmailRenderer
	{
		private const string EmailViewPath = "~/views/partials/formeditor/email/";

		private class FakeController : Controller
		{
		}

		public static string Render(string viewName, FormModel model, IPublishedContent currentContent)
		{
			var viewPath = Path.Combine(EmailViewPath, viewName);
			var viewFile = new FileInfo(HostingEnvironment.MapPath(viewPath));
			if (viewFile.Exists == false)
			{
				return null;
			}
			var writer = new StringWriter();
			var context = new HttpContextWrapper(HttpContext.Current);
			var routeData = new RouteData();
            		routeData.Values["controller"] = "FakeController";
			var controllerContext = new ControllerContext(new RequestContext(context, routeData), new FakeController());
			var razor = new RazorView(controllerContext, viewPath, null, false, null);
			var viewData = new ViewDataDictionary(model);
			// pass the current content to the email template, because Umbraco.AssignedContentItem doesn't work out of Umbraco context
			viewData["currentContent"] = currentContent;
			razor.Render(new ViewContext(controllerContext, razor, viewData, new TempDataDictionary(), writer), writer);
			return writer.ToString();
		}

		public static IEnumerable<string> GetAllEmailTemplates()
		{
			var directory = new DirectoryInfo(HostingEnvironment.MapPath(EmailViewPath));
			if (directory.Exists == false)
			{
				return new string[] { };
			}
			return directory.GetFiles("*.cshtml").Select(f => f.Name);
		}
	}
}
