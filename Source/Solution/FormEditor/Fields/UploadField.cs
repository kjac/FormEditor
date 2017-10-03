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
		public override string PrettyName => "File upload";

		public override string Type => "core.upload";

		public int MaxSize { get; set; }

		// comma separated list of file types
		public string AllowedFileTypes { get; set; }

		public string GetValidExtensions()
		{
			var validExtensions = ParseAllowedFileTypes();
			return validExtensions.Any()
				? $".{string.Join(", .", validExtensions)}"
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
			var newFilename = $"{Guid.NewGuid()}.upload";

			// index value will be "[original filename]|[filename on disk]|[file size in bytes]"
			SubmittedValue = $"{originalFilename}|{newFilename}|{contentLength}";
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
			var maxSize = MaxSize*1024;
			if(maxSize > 0 && file.ContentLength > maxSize)
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
			if (file == null || file.ContentLength == 0)
			{
				return null;
			}
			return file;
		}

		protected internal override string FormatSubmittedValueForIndex(IPublishedContent content, Guid rowId)
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
			if (index.SaveFile(file, indexValue.PersistedFilename, rowId) == false)
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

			// larger than one MB?
			var fileSize = fileData.FileSize >= 1048576
				// yes, list as MB
				? $"{(((double) fileData.FileSize) / 1048576):0.0} MB"
				// no, list as KB
				: $"{(((double) fileData.FileSize) / 1024):0.0} KB";

			return 
$@"<a href=""/umbraco/backoffice/FormEditorApi/Download/DownloadFile/{content.Id}?rowId={rowId}&fieldName={FormSafeName}"" onclick=""event.cancelBubble=true;"" class=""downloadFile"">
	<i class=""icon icon-download-alt""></i><small>{fileSize}</small>
</a>";
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
		public override bool SupportsStripHtml => false;

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
			if (int.TryParse(tokens[2], out var fileSize) == false)
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