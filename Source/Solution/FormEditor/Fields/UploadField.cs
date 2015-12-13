using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using FormEditor.Storage;
using Umbraco.Core.Models;

namespace FormEditor.Fields
{
	public class UploadField : FieldWithMandatoryValidation
	{
		public override string PrettyName
		{
			get { return "File upload"; }
		}
		public override string Type
		{
			get { return "core.upload"; }
		}
		public int MaxSize { get; set; }

		// comma separated list of file types
		public string AllowedFileTypes { get; set; }

		public string GetValidExtensions()
		{
			var validExtensions = ParseAllowedFileTypes();
			return validExtensions.Any()
				? string.Format(".{0}", string.Join(", .", validExtensions))
				: null;
		}

		private string[] ParseAllowedFileTypes()
		{
			if (string.IsNullOrEmpty(AllowedFileTypes))
			{
				return new string[0];
			}
			// this is the hacky way of cleaning up the list of allowed file types, so "*." and "." prefixes aren't taken into account
			return AllowedFileTypes
				.Split(new[] { ',', ' ', '*', '.' }, StringSplitOptions.RemoveEmptyEntries)
				.ToArray();
		}

		protected internal override void CollectSubmittedValue(Dictionary<string, string> allSubmittedValues, IPublishedContent content)
		{
			var file = GetUploadedFile();
			if (file == null)
			{
				SubmittedValue = null;
				return;
			}

			var contentLength = file.ContentLength;
			var originalFilename = file.FileName;
			var newFilename = string.Format("{0}.upload", Guid.NewGuid());

			// index value will be "[original filename]|[filename on disk]|[file size in bytes]"
			SubmittedValue = string.Format("{0}|{1}|{2}", originalFilename, newFilename, contentLength);
		}

		protected internal override bool ValidateSubmittedValue(IEnumerable<Field> allCollectedValues, IPublishedContent content)
		{
			var file = GetUploadedFile();
			if (file == null)
			{
				// no file uploaded => invalid if mandatory
				return Mandatory == false;
			}

			// validate file size
			if (MaxSize > 0 && file.ContentLength > MaxSize)
			{
				return false;
			}
			// validate file type
			var validExtensions = ParseAllowedFileTypes();
			if (validExtensions.Any())
			{
				var fileInfo = new FileInfo(file.FileName);
				if (validExtensions.Contains(fileInfo.Extension.TrimStart('.'), StringComparer.InvariantCultureIgnoreCase) == false)
				{
					return false;
				}
			}
			return true;
		}

		private HttpPostedFile GetUploadedFile()
		{
			if (HttpContext.Current.Request.Files.AllKeys.Contains(FormSafeName) == false)
			{
				return null;
			}
			var file = HttpContext.Current.Request.Files[FormSafeName];
			if (file.ContentLength == 0)
			{
				return null;
			}
			return file;
		}

		protected internal override string FormatSubmittedValueForIndex(IPublishedContent content)
		{
			var file = GetUploadedFile();
			if (file == null)
			{
				return null;
			}

			var indexValue = ParseIndexValue(SubmittedValue);
			if (indexValue == null)
			{
				return null;
			}

			// save the file to the index
			var index = IndexHelper.GetIndex(content.Id);
			if (index.SaveFile(file, indexValue.PersistedFilename) == false)
			{
				return null;
			}

			return SubmittedValue;
		}

		protected internal override string FormatValueForDataView(string value, IContent content, Guid rowId)
		{
			// parse index value and return download link for /umbraco/backoffice/FormEditorApi/Download/DownloadFile
			var fileData = ParseIndexValue(value);
			if (fileData == null)
			{
				// no value
				return base.FormatValueForDataView(value, content, rowId);
			}

			return string.Format(
@"<a href=""/umbraco/backoffice/FormEditorApi/Download/DownloadFile/{0}?rowId={1}&fieldName={2}"" onclick=""event.cancelBubble=true;"" class=""downloadFile"">
	<i class=""icon icon-download-alt""></i><span class=""menu-label"">{3}</span>
</a>",
				content.Id,
				rowId,
				FormSafeName,
				string.Format(@"{0}",
					// larger than one MB?
					fileData.FileSize >= 1048576
						// yes, list as MB
						? string.Format("{0:0.0} MB", (((double)fileData.FileSize) / 1048576))
						// no, list as KB
						: string.Format("{0:0.0} KB", (((double)fileData.FileSize) / 1024))
				)
			);
		}

		protected internal override string FormatValueForCsvExport(string value, IContent content, Guid rowId)
		{
			// parse index value and return original file name
			var fileData = ParseIndexValue(value);
			if (fileData == null)
			{
				// no value
				return base.FormatValueForDataView(value, content, rowId);
			}
			return fileData.OriginalFilename;
		}

		protected internal override string FormatValueForFrontend(string value, IPublishedContent content, Guid rowId)
		{
			// parse index value and return original file name
			var fileData = ParseIndexValue(value);
			if (fileData == null)
			{
				// no value
				return base.FormatValueForFrontend(value, content, rowId);
			}
			return fileData.OriginalFilename;
		}

		public override string SubmittedValueForEmail()
		{
			// parse submitted value and return original file name
			var fileData = ParseIndexValue(SubmittedValue);
			if (fileData == null)
			{
				// no value
				return base.SubmittedValueForEmail();
			}
			return fileData.OriginalFilename;
		}

		// let's just make sure the upload field is never sanitized
		public override bool SupportsStripHtml
		{
			get { return false; }
		}

		public static FileData ParseIndexValue(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			var tokens = value.Split('|');
			if (tokens.Length != 3)
			{
				return null;
			}
			int fileSize;
			if (int.TryParse(tokens[2], out fileSize) == false)
			{
				return null;
			}
			return new FileData
			{
				OriginalFilename = tokens[0],
				PersistedFilename = tokens[1],
				FileSize = fileSize
			};
		}

		public class FileData
		{
			public string OriginalFilename { get; set; }
			public string PersistedFilename { get; set; }
			public int FileSize { get; set; }
		}
	}
}