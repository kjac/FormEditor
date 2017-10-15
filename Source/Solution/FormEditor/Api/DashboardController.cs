using System;
using System.Collections.Generic;
using System.Linq;
using FormEditor.Storage;
using FormEditor.Umbraco;
using Umbraco.Core.Models;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace FormEditor.Api
{
	[PluginController("FormEditorApi")]
	public class DashboardController : UmbracoAuthorizedJsonController
	{
		// TODO: proper HttpResponseMessage return value + lots of error handling
		public object SearchAll(string query)
		{
			var contentResults = new List<ContentResult>();
			ContentHelper.ForEachFormModel(ApplicationContext.Services, (formModel, content) =>
			{
				if(!(IndexHelper.GetIndex(content.Id) is IFullTextIndex index))
				{
					return;
				}

				var fields = formModel.AllValueFields().ToArray();
				var result = index.Search(query, fields.Select(f => f.FormSafeName).ToArray(), null, false, 100, 0);
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

			return contentResults;
		}

		private class ContentResult
		{
			public int ContentId { get; set; }

			public string ContentName { get; set; }

			public string[] FieldNames { get; set; }

			public Data.Row[] Rows { get; set; }
		}
	}
}
