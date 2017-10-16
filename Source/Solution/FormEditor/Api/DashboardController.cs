using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using FormEditor.Storage;
using FormEditor.Umbraco;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace FormEditor.Api
{
	[PluginController("FormEditorApi")]
	public class DashboardController : UmbracoAuthorizedJsonController
	{
		[HttpGet]
		public HttpResponseMessage SearchAll(string searchQuery)
		{
			var contentResults = new List<ContentResult>();
			ContentHelper.ForEachFormModel(ApplicationContext.Services, (formModel, content) =>
			{
				if(!(IndexHelper.GetIndex(content.Id) is IFullTextIndex index))
				{
					return;
				}

				var fields = formModel.AllValueFields().ToArray();
				var result = index.Search(searchQuery, fields.Select(f => f.FormSafeName).ToArray(), null, false, 100, 0);
				if(result == null || result.TotalRows == 0)
				{
					return;
				}

				var rows = formModel.ExtractSubmittedValues(result, fields, (field, value, row) => value == null ? null : field.FormatValueForDataView(value, content, row.Id));

				contentResults.Add(new ContentResult
				{
					ContentId = content.Id,
					ContentName = content.Name,
					FieldNames = fields.Select(f => f.Name).ToArray(),
					Rows = rows.ToArray()
				});
			});

			var formatter = new JsonMediaTypeFormatter
			{
				SerializerSettings =
				{
					ContractResolver = new CamelCasePropertyNamesContractResolver(),
					TypeNameHandling = TypeNameHandling.None
				}
			};
			return Request.CreateResponse(HttpStatusCode.OK, contentResults, formatter);
		}

		[HttpPost]
		public HttpResponseMessage Delete(DeleteRequest request)
		{
			var index = IndexHelper.GetIndex(request.ContentId);
			index.Remove(request.RowIds);
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		private class ContentResult
		{
			public int ContentId { get; set; }

			public string ContentName { get; set; }

			public string[] FieldNames { get; set; }

			public Data.Row[] Rows { get; set; }
		}

		public class DeleteRequest
		{
			public int ContentId { get; set; }

			public IEnumerable<Guid> RowIds { get; set; }
		}
	}
}
