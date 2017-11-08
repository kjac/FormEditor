using Umbraco.Core.Models;
using Umbraco.Web;

namespace FormEditor.Fields
{
	public class ImageField : Field
	{
		public override string Type => "core.image";

		public override string PrettyName => "Image";

		public string Text { get; set; }

		public int MediaId { get; set; }

		public IPublishedContent Media
		{
			get
			{
				if (MediaId <= 0)
				{
					return null;
				}
				return UmbracoContext.Current.MediaCache.GetById(MediaId);
			}
		}
	}
}