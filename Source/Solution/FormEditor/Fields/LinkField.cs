using Umbraco.Core.Models;
using Umbraco.Web;

namespace FormEditor.Fields
{
	public class LinkField : Field
	{
		// local cache for multiple in-request access to the Page propery
		private IPublishedContent _page;

		public override string Type => "core.link";

		public override string PrettyName => "Link";

		public string Text { get; set; }

		public int PageId { get; set; }

		public bool OpenInNewWindow { get; set; }

		public IPublishedContent Page
		{
			get
			{
				if(PageId <= 0)
				{
					return null;
				}
				if (_page != null)
				{
					return _page;
				}
				_page = UmbracoContext.Current.ContentCache.GetById(PageId);
				return _page;
			}
		}
	}
}