using Umbraco.Core.Models;
using Umbraco.Web;

namespace FormEditor.Fields
{
	public class LinkField : Field
	{
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
				return UmbracoContext.Current.ContentCache.GetById(PageId);
			}
		}
	}
}