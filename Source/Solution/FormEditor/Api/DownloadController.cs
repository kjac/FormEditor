using System.Collections.Generic;
﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using FormEditor.Fields;
using FormEditor.Storage;
using FormEditor.Umbraco;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace FormEditor.Api
{
	[PluginController("FormEditorApi")]
	public class DownloadController : UmbracoAuthorizedApiController
	{
		[HttpGet]
		public HttpResponseMessage DownloadFile(int id, Guid rowId, string fieldName)
		{
			var index = IndexHelper.GetIndex(id);
			var row = index.Get(rowId);
			if (row == null || row.Fields.ContainsKey(fieldName) == false)
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}

			var indexValue = row.Fields[fieldName];
			var fileData = UploadField.ParseIndexValue(indexValue);
			if (fileData == null)
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}

			var stream = index.GetFile(fileData.PersistedFilename, rowId);
			if (stream == null)
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}

			var response = new HttpResponseMessage(HttpStatusCode.OK);
			response.Content = new StreamContent(stream);
			response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
			response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = fileData.OriginalFilename
			};
			return response;
		}

		[HttpGet]
		public HttpResponseMessage DownloadData(int id)
		{
			return DownloadData(id, null);
		}

		[HttpGet]
		public HttpResponseMessage DownloadData(int id, string rowIds)
		{
			var document = ContentHelper.GetById(id);
			var model = ContentHelper.GetFormModel(document);
			if (document == null)
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}
			if (model == null)
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}
			var allFields = PropertyEditorController.GetAllFieldsForDisplay(model, document);

			var selectedRows = string.IsNullOrEmpty(rowIds) ? null : rowIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList();

			var index = IndexHelper.GetIndex(id);

			List<Storage.Row> rows = null;

			if (selectedRows != null && selectedRows.Any())
			{
				rows = index.Get(selectedRows).ToList();
			}
			else
			{
				var result = index.Get("_created", false, 100000, 0);
				if (result != null && result.Rows != null)
				{
					rows = result.Rows.ToList();
				}
			}

			if (rows == null || rows.Any() == false)
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}

			var csv = new CsvExport();
			foreach (var row in rows)
			{
				csv.AddRow();
				// add date to blank header column
				csv[""] = row.CreatedDate;
				foreach (var modelField in allFields)
				{
					if (row.Fields.Any(f => f.Key == modelField.FormSafeName))
					{
						csv[modelField.Name] = modelField.FormatValueForCsvExport(
							row.Fields.First(f => f.Key == modelField.FormSafeName).Value,
							document,
							row.Id
						);
					}
					else
					{
						csv[modelField.Name] = string.Empty;
					}
				}
			}

			var response = new HttpResponseMessage(HttpStatusCode.OK);
			var stream = new System.IO.MemoryStream(csv.ExportToBytes());
			response.Content = new StreamContent(stream);
			response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
			response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = string.Format(@"Form data {0}.csv", DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
			};
			return response;
		}
	}
}
