using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using FormEditor.Data;
using FormEditor.Events;
using FormEditor.Fields;
using FormEditor.Fields.Statistics;
using FormEditor.Storage;
using FormEditor.Storage.Statistics;
using Umbraco.Core.Models;
using Umbraco.Web;
using Field = FormEditor.Fields.Field;

namespace FormEditor
{
	public class FormModel
	{
		public const string PropertyEditorAlias = @"FormEditor.Form";

		private const string FormSubmittedCookieKey = "_fe";

		private IEnumerable<Page> _pages;
		private IEnumerable<Row> _rows;

		#region Properties configured in the form editor

		public IEnumerable<Page> Pages
		{
			get
			{
				EnsurePagesForBackwardsCompatibility();
				return _pages;
			}
			set
			{
				_pages = value;
			}
		}

		public IEnumerable<Row> Rows
		{
			get
			{
				EnsureRowsForBackwardsCompatibility();
				return _rows;
			}
			set { _rows = value; }
		}

		public IEnumerable<Validation.Validation> Validations { get; set; }
		public Guid RowId { get; set; }
		public string EmailNotificationRecipients { get; set; }
		public string EmailNotificationSubject { get; set; }
		public string EmailNotificationFromAddress { get; set; }
		public bool EmailNotificationAttachments { get; set; }
		public string EmailConfirmationRecipientsField { get; set; }
		public string EmailConfirmationSubject { get; set; }
		public string EmailConfirmationBody { get; set; }
		public string EmailConfirmationFromAddress { get; set; }
		public int SuccessPageId { get; set; }
		public string ReceiptHeader { get; set; }
		public string ReceiptBody { get; set; }
		public int? MaxSubmissions { get; set; }
		public string MaxSubmissionsExceededHeader { get; set; }
		public string MaxSubmissionsExceededText { get; set; }
		public bool DisallowMultipleSubmissionsPerUser { get; set; }
		public string MaxSubmissionsForCurrentUserExceededHeader { get; set; }
		public string MaxSubmissionsForCurrentUserExceededText { get; set; }

		#endregion

		#region Properties configured in the prevalues

		private string EmailNotificationTemplate { get; set; }
		private string EmailConfirmationTemplate { get; set; }
		private bool LogIp { get; set; }
		private bool StripHtml { get; set; }
		// TODO: remove this in an upcoming release (obsolete)
		private bool DisableValidation { get; set; }
		private bool UseStatistics { get; set; }

		#endregion

		// events
		public static event FormEditorCancelEventHandler BeforeAddToIndex;
		public static event FormEditorEventHandler AfterAddToIndex;

		public bool CollectSubmittedValues(bool redirect = true)
		{
			if(RequestedContent == null)
			{
				return false;
			}
			return CollectSubmittedValues(RequestedContent, redirect);
		}

		public bool CollectSubmittedValues(IPublishedContent content, bool redirect = true)
		{
			if(content == null)
			{
				return false;
			}
			
			// currently not supporting GET forms ... will require some limitation on fields and stuff
			if(Request.HttpMethod != "POST")
			{
				return false;
			}

			// does the form contain an "_id" and if so, does it match the supplied content?
			int id;
			if(int.TryParse(HttpContext.Current.Request.Form["_id"], out id) && id != content.Id)
			{
				return false;
			}

			// are we able to accept any submissions for this form for the current user?
			if (MaxSubmissionsExceededForCurrentUser(content))
			{
				return false;
			}

			// are we able to accept any submissions for this form at all?
			if(MaxSubmissionsExceeded(content))
			{
				return false;
			}

			// first collect the submitted values
			var fields = CollectSubmittedValuesFromRequest(content);

			// next validate the submitted values
			if(ValidateSubmittedValues(content, fields) == false)
			{
				return false;
			}

			// load the data type prevalues for various settings (log IP, email templates etc)
			LoadPreValues(content);

			// get all "fields with value"
			var valueFields = AllValueFields().ToList();

			// next execute all validations (if validation is enabled)
			if(DisableValidation == false && ExecuteValidations(content, valueFields) == false)
			{
				return false;
			}

			// we're about to add data to the index - let's run the before add to index event handler
			var beforeAddToIndexErrorMessage = RaiseBeforeAddToIndex(content);
			if(beforeAddToIndexErrorMessage != null)
			{
				// the event was cancelled - use the validation system to pass the error message back to the user
				Validations = new List<Validation.Validation>(Validations ?? new Validation.Validation[] { })
				{
					new Validation.Validation
					{
						ErrorMessage = beforeAddToIndexErrorMessage, 
						Invalid = true, 
						Rules = new Validation.Rule[] {}
					}
				};
				return false;
			}

			// before add to index event handling did not cancel - add to index
			var rowId = AddSubmittedValuesToIndex(content, valueFields);
			if(rowId == Guid.Empty)
			{
				return false;
			}

			// tell everyone that something was added
			RaiseAfterAddToIndex(rowId, content);

			SetFormSubmittedCookie(content);

			SendEmails(content, valueFields);

			if (redirect && SuccessPageId > 0)
			{
				RedirectToSuccesPage();
			}

			return true;
		}

		public FormData GetSubmittedValues(int page = 1, int perPage = 10, string sortField = null, bool sortDescending = false)
		{
			if(RequestedContent == null)
			{
				return new FormData();
			}
			return GetSubmittedValues(RequestedContent, page, perPage, sortField, sortDescending);
		}

		public FormData GetSubmittedValues(IPublishedContent content, int page = 1, int perPage = 10, string sortField = null, bool sortDescending = false)
		{
			var index = IndexHelper.GetIndex(content.Id);
			var result = index.Get(sortField, sortDescending, perPage, (page - 1) * perPage) ?? Result.Empty(sortField, sortDescending);
			var fields = AllValueFields();

			var rows = ExtractSubmittedValues(result, fields, (field, value, row) => value == null ? null : field.FormatValueForFrontend(value, content, row.Id));

			return new FormData
			{
				TotalRows = result.TotalRows,
				SortDescending = result.SortDescending,
				SortField = result.SortField,
				Rows = rows,
				Fields = fields.Select(f => ToDataField(null, f, null))
			};
		}

		public void LoadValues(Guid rowId)
		{
			if (RequestedContent == null)
			{
				return;
			}
			LoadValues(RequestedContent, rowId);
		}

		public void LoadValues(IPublishedContent content, Guid rowId)
		{
			if (rowId == Guid.Empty)
			{
				return;
			}
			var index = IndexHelper.GetIndex(content.Id);
			var formData = index.Get(rowId);

			if (formData == null)
			{
				return;
			}

			RowId = rowId;

			var fields = AllFields().ToArray();
			foreach (var field in fields)
			{
				field.CollectSubmittedValue(formData.Fields, content);
			}
			foreach (var field in fields)
			{
				// using ValidateSubmittedValue to load up select boxes
				field.ValidateSubmittedValue(fields, content);
			}
		}

		public IEnumerable<Field> AllFields()
		{
			return Pages.SelectMany(p => p.AllFields());
		}

		public IEnumerable<FieldWithValue> AllValueFields()
		{
			return AllFields().OfType<FieldWithValue>();
		}

		public bool MaxSubmissionsExceededForCurrentUser()
		{
			if (RequestedContent == null)
			{
				return true;
			}
			return MaxSubmissionsExceededForCurrentUser(RequestedContent);
		}

		public bool MaxSubmissionsExceededForCurrentUser(IPublishedContent content)
		{
			if (DisallowMultipleSubmissionsPerUser == false)
			{
				return false;
			}
			var cookie = Request.Cookies[FormSubmittedCookieKey];
			return cookie != null && cookie.Value.Contains(FormSubmittedCookieValue(content));
		}

		public bool MaxSubmissionsExceeded()
		{
			if(RequestedContent == null)
			{
				return true;
			}
			return MaxSubmissionsExceeded(RequestedContent);
		}

		public bool MaxSubmissionsExceeded(IPublishedContent content)
		{
			if(MaxSubmissions.HasValue == false)
			{
				return false;
			}
			var index = IndexHelper.GetIndex(content.Id);
			return index.Count() >= MaxSubmissions.Value;
		}

		public FieldValueFrequencyStatistics<IStatisticsField> GetFieldValueFrequencyStatistics(IEnumerable<string> fieldNames = null)
		{
			return GetFieldValueFrequencyStatistics(RequestedContent, fieldNames);
		}

		public FieldValueFrequencyStatistics<IStatisticsField> GetFieldValueFrequencyStatistics(IPublishedContent content, IEnumerable<string> fieldNames = null)
		{
			if(content == null)
			{
				return new FieldValueFrequencyStatistics<IStatisticsField>(0);
			}
			var fields = AllValueFields().StatisticsFields();
			if(fieldNames != null)
			{
				fieldNames = fields.StatisticsFieldNames().Intersect(fieldNames, StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				fieldNames = fields.StatisticsFieldNames();
			}
			if(fieldNames.Any() == false)
			{
				return new FieldValueFrequencyStatistics<IStatisticsField>(0);
			}
			var index = IndexHelper.GetIndex(content.Id) as IStatisticsIndex;
			if(index == null)
			{
				return new FieldValueFrequencyStatistics<IStatisticsField>(0);
			}
			var statistics = index.GetFieldValueFrequencyStatistics(fieldNames);
			// the statistics are indexed by field.FormSafeName - we need to reindex them by 
			// the fields themselves to support the frontend rendering 
			var result = new FieldValueFrequencyStatistics<IStatisticsField>(statistics.TotalRows);
			foreach(var fieldValueFrequency in statistics.FieldValueFrequencies)
			{
				var field = fields.FirstOrDefault(f => f.FormSafeName == fieldValueFrequency.Field);
				if(field == null)
				{
					continue;
				}
				result.Add(field, fieldValueFrequency.Frequencies);
			}
			return result;
		}

		private HttpRequest Request
		{
			get { return HttpContext.Current.Request; }
		}

		private HttpResponse Response
		{
			get { return HttpContext.Current.Response; }
		}

		private UmbracoContext Context
		{
			get { return UmbracoContext.Current; }
		}

		private IPublishedContent RequestedContent
		{
			get
			{
				if(Context == null || Context.PublishedContentRequest == null || Context.PublishedContentRequest.PublishedContent == null)
				{
					return null;
				}
				return Context.PublishedContentRequest.PublishedContent;
			}
		}

		#region Stuff for backwards compatibility with v0.10.0.2 (before introducing form pages) - should probably be removed at some point

		private void EnsurePagesForBackwardsCompatibility()
		{
			if(_pages == null && _rows != null)
			{
				_pages = new List<Page>
				{
					new Page
					{
						Rows = _rows
					}
				};
			}
		}

		private void EnsureRowsForBackwardsCompatibility()
		{
			if(_rows == null && _pages != null)
			{
				_rows = _pages.SelectMany(p => p.Rows).ToList();
			}
		}

		#endregion

		#region Collect submitted values

		private List<Field> CollectSubmittedValuesFromRequest(IPublishedContent content)
		{
			var valueCollection = Request.Form;
			var allSubmittedValues = valueCollection.AllKeys.ToDictionary(k => k, k => TryGetSubmittedValue(k, valueCollection));

			var fields = AllFields().ToList();

			// first collect the submitted values
			foreach(var field in fields)
			{
				field.CollectSubmittedValue(allSubmittedValues, content);
			}
			return fields;

		}

		private static string TryGetSubmittedValue(string k, NameValueCollection valueCollection)
		{
			try
			{
				return valueCollection[k];
			}
			catch (HttpRequestValidationException ex)
			{
				// guard for "A potentially dangerous Request.Form value was detected from the client"
				Log.Warning(@"HTTP request validation failed for form key ""{0}"" with the following message: {1}", k, ex.Message);
				return string.Empty;
			}
		}

		private static bool ValidateSubmittedValues(IPublishedContent content, List<Field> fields)
		{
			foreach(var field in fields)
			{
				field.Invalid = (field.ValidateSubmittedValue(fields, content) == false);
			}
			return fields.Any(f => f.Invalid) == false;
		}

		private bool ExecuteValidations(IPublishedContent content, IEnumerable<FieldWithValue> valueFields)
		{
			if(Validations != null
			   && Validations.Any()
				// if any validation fails, the entire form validation fails
				// - instead of calling Validations.All() here, this actually forces all validations to execute
			   && Validations.Count(v => v.IsValidFor(valueFields, content) == false) != 0)
			{
				return false;
			}
			return true;
		}

		private Guid AddSubmittedValuesToIndex(IPublishedContent content, IEnumerable<FieldWithValue> valueFields)
		{
			// we're performing an update if RowId as a value
			var isUpdate = RowId != Guid.Empty;
			// generate a new row ID for the index only if we're not performing an update, otherwise reuse RowId
			var indexRowId = isUpdate ? RowId : Guid.NewGuid();

			// get the storage index
			var index = IndexHelper.GetIndex(content.Id);
			// - attempt to cast to IStatisticsIndex if statistics are enabled
			var statisticsIndex = UseStatistics ? index as IStatisticsIndex : null;
			// - attempt to cast to IUpdateIndex if we're performing an update 
			//   (this will change in an upcoming release when IUpdateIndex is merged into IIndex)
			var updateIndex = isUpdate ? index as IUpdateIndex : null;

			if (isUpdate)
			{
				// can we perform the update? only IStatisticsIndex and IUpdateIndex support updates
				if (statisticsIndex == null && updateIndex == null)
				{
					return Guid.Empty;
				}
			}

			// extract all index values
			var indexFields = valueFields.ToDictionary(f => f.FormSafeName, f => FormatForIndexAndSanitize(f, content, indexRowId));

			// add the IP of the user if enabled on the data type
			if(LogIp)
			{
				indexFields.Add("_ip", Request.UserHostAddress);
			}

			if (statisticsIndex != null)
			{
				var indexFieldsForStatistics = valueFields.StatisticsFields().ToDictionary(f => f.FormSafeName, f => f.SubmittedValues ?? new string[] {});
				return isUpdate
					? statisticsIndex.Update(indexFields, indexFieldsForStatistics, indexRowId)
					: statisticsIndex.Add(indexFields, indexFieldsForStatistics, indexRowId);
			}
			if (updateIndex != null)
			{
				return updateIndex.Update(indexFields, indexRowId);
			}

			return index.Add(indexFields, indexRowId);			
		}
		
		private string FormatForIndexAndSanitize(FieldWithValue field, IPublishedContent content, Guid rowId)
		{
			var value = field.FormatSubmittedValueForIndex(content, rowId);
			if (string.IsNullOrWhiteSpace(value) || StripHtml == false || field.SupportsStripHtml == false)
			{
				return value;
			}
			var doc = new HtmlAgilityPack.HtmlDocument { OptionAutoCloseOnEnd = true };
			doc.LoadHtml(value);
			return doc.DocumentNode.InnerText;
		}

		private string RaiseBeforeAddToIndex(IPublishedContent content)
		{
			if(BeforeAddToIndex != null)
			{
				try
				{
					var cancelEventArgs = new FormEditorCancelEventArgs(content);
					BeforeAddToIndex.Invoke(this, cancelEventArgs);
					if(cancelEventArgs.Cancel)
					{
						Log.Info("The form submission was valid, but it was not added to the index because an event handler for BeforeAddToIndex cancelled the submission.");
						return cancelEventArgs.ErrorMessage ?? "The form submission was cancelled by the BeforeAddToIndex event handler.";
					}
				}
				catch(Exception ex)
				{
					// an event handler failed - log error and continue
					Log.Error(ex, "An event handler for BeforeAddToIndex threw an exception.");
				}
			}
			return null;
		}

		private void RaiseAfterAddToIndex(Guid rowId, IPublishedContent content)
		{
			if(AfterAddToIndex != null)
			{
				try
				{
					AfterAddToIndex.Invoke(this, new FormEditorEventArgs(rowId, content));
				}
				catch(Exception ex)
				{
					// an event handler failed - log error and continue
					Log.Error(ex, "An event handler for AfterAddToIndex threw an exception.");
				}
			}
		}

		private void SendEmails(IPublishedContent content, IEnumerable<FieldWithValue> valueFields)
		{
			string emailBody = null;

			if(string.IsNullOrWhiteSpace(EmailNotificationRecipients) == false)
			{
				// extract uploaded files for mail attachments if this has been chosen by the editor.
				// NOTE: it's tempting to use valueFields.OfType<UploadField>() for this, but then we'd explicitly be leaving out any 
				// custom file upload fields. so instead we'll run through the files in the request and match their names against 
				// the value field names.
				var uploadedFiles = EmailNotificationAttachments 
					? Request.Files.AllKeys
						.Where(k => valueFields.Any(f => f.FormSafeName == k))
						.Select(k => Request.Files[k])
						.Where(f => f != null && f.ContentLength > 0)
						.ToArray()
					: null;
				SendEmailType(EmailNotificationSubject, EmailNotificationFromAddress, EmailNotificationRecipients, content, EmailNotificationTemplate, "Notification", uploadedFiles, ref emailBody);
			}

			if(string.IsNullOrWhiteSpace(EmailConfirmationRecipientsField))
			{
				return;
			}

			var recipientsField = valueFields.FirstOrDefault(f => f.Name == EmailConfirmationRecipientsField);
			if(recipientsField == null)
			{
				Log.Warning("Confirmation email could not be sent because the field containing the email recipient ({0}) could not be found.", EmailConfirmationRecipientsField);
			}
			else
			{
				var emailField = recipientsField as IEmailField;
				var recipientAddresses = emailField != null
					? string.Join(",", emailField.EmailAddresses ?? new string[0])
					: recipientsField.SubmittedValue;

				// can we reuse the rendered email body?
				if(EmailConfirmationTemplate != EmailNotificationTemplate)
				{
					// nope
					emailBody = null;
				}
				SendEmailType(EmailConfirmationSubject, EmailConfirmationFromAddress, recipientAddresses, content, EmailConfirmationTemplate, "Confirmation", null, ref emailBody);
			}
		}

		private void SendEmailType(string subject, string senderAddress, string recipientAddresses, IPublishedContent currentContent, string template, string emailType, HttpPostedFile[] uploadedFiles, ref string emailBody)
		{
			if(string.IsNullOrEmpty(template))
			{
				Log.Warning("{0} email could not be sent because the email template was empty.", emailType);
				return;	
			}

			var senderEmailAddress = ParseEmailAddresses(senderAddress).FirstOrDefault();
			if(senderEmailAddress == null)
			{
				Log.Warning("{0} email could not be sent because the email sender address ({1}) was not valid.", emailType, senderAddress);
				return;
			}

			var addresses = ParseEmailAddresses(recipientAddresses);
			if(addresses.Any() == false)
			{
				Log.Warning("{0} email was not be sent because no valid recipient email addresses was found.", emailType);
				return;				
			}
			if(emailBody == null)
			{
				try
				{
					// render the email body 
					emailBody = EmailRenderer.Render(template, this, currentContent);
				}
				catch(Exception ex)
				{
					Log.Error(ex, "{0} mail template ({1}) could not be rendered, see exception for details.", emailType, template);
					return;
				}				
			}

			// interpolate submitted values in the email subject 
			subject = InterpolateSubmittedValues(subject);

			// send emails to the recipients
			SendEmails(subject, emailBody, senderEmailAddress, addresses, uploadedFiles);
		}

		private static void SendEmails(string subject, string body, MailAddress from, IEnumerable<MailAddress> to, HttpPostedFile[] uploadedFiles)
		{
			if(string.IsNullOrEmpty(body))
			{
				return;
			}
			var mail = new MailMessage
			{
				From = from,
				// #23 - explicitly set Reply-To field
				ReplyToList = { from },
				Subject = subject ?? string.Empty,
				Body = body,
				IsBodyHtml = body.Contains("<html") || body.Contains("<body") || body.Contains("<div")
			};
			foreach(var address in to)
			{
				mail.To.Add(address);
			}

			// we need to load the uploaded files into memory because they're disposed when the request ends
			var attachments = new List<FileAttachment>();
			foreach(var file in uploadedFiles ?? new HttpPostedFile[0])
			{
				var attachment = new FileAttachment
				{
					Name = file.FileName,
					ContentType = file.ContentType,
					Bytes = new byte[file.ContentLength]
				};
				file.InputStream.Read(attachment.Bytes, 0, file.ContentLength);
				attachments.Add(attachment);
			}

			// send the mail as fire-and-forget (pass the attachments as state)
			ThreadPool.QueueUserWorkItem(state =>
			{
				foreach(var attachment in (IEnumerable<FileAttachment>)state)
				{
					mail.Attachments.Add(new Attachment(new MemoryStream(attachment.Bytes), attachment.Name, attachment.ContentType));
				}
				try
				{
					var client = new SmtpClient();
					client.Send(mail);
				}
				catch(Exception ex)
				{
					// NOTE: we're out of the request context here, but the logging still seems to work just fine
					Log.Error(ex, "Email could not be sent, see exception for details.");
				}

			}, attachments);
		}

		private List<MailAddress> ParseEmailAddresses(string emails)
		{
			var addresses = new List<MailAddress>();
			foreach(var email in (emails ?? string.Empty).Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries))
			{
				try
				{
					var address = new MailAddress(email);
					addresses.Add(address);
				}
				catch
				{
					Log.Info("Could not parse \"{0}\" as a valid email address.", email);
				}
			}
			return addresses;
		}

		private void LoadPreValues(IPublishedContent content)
		{
			try
			{
				if(content == null)
				{
					return;
				}
				var property = content.ContentType.PropertyTypes.FirstOrDefault(p => p.PropertyEditorAlias == PropertyEditorAlias);
				if(property == null)
				{
					return;
				}
				var preValues = Context.Application.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(property.DataTypeId);
				if(preValues == null)
				{
					return;
				}
				var preValueDictionary = preValues.PreValuesAsDictionary;
				EmailNotificationTemplate = GetPreValue("notificationEmailTemplate", preValueDictionary);
				EmailConfirmationTemplate = GetPreValue("confirmationEmailTemplate", preValueDictionary);
				LogIp = GetPreValueAsBoolean("logIp", preValueDictionary);
				StripHtml = GetPreValueAsBoolean("stripHtml", preValueDictionary);
				DisableValidation = GetPreValueAsBoolean("disableValidation", preValueDictionary);
				UseStatistics = GetPreValueAsBoolean("enableStatistics", preValueDictionary);
			}
			catch(Exception ex)
			{
				Log.Error(ex, "Could not load prevalues for property editor, see exception for details.");
			}
		}

		private static string GetPreValue(string key, IDictionary<string, PreValue> preValueDictionary)
		{
			return preValueDictionary.ContainsKey(key) && preValueDictionary[key] != null
				? preValueDictionary[key].Value
				: null;
		}

		private static bool GetPreValueAsBoolean(string key, IDictionary<string, PreValue> preValueDictionary)
		{
			return GetPreValue(key, preValueDictionary) == "1";
		}

		private void RedirectToSuccesPage()
		{
			var helper = new UmbracoHelper(Context);
			var redirectTo = helper.TypedContent(SuccessPageId);
			if(redirectTo != null)
			{
				HttpContext.Current.Response.Redirect(redirectTo.Url);
			}
		}

		private class FileAttachment
		{
			public string Name { get; set; }
			public string ContentType { get; set; }
			public byte[] Bytes { get; set; }
		}

		public IHtmlString GetEmailConfirmationBodyText(bool forHtmlEmail = true)
		{
			var emailBody = InterpolateSubmittedValues(EmailConfirmationBody);

			// replace newlines with <br/> in email body for HTML mails
			return new HtmlString(forHtmlEmail ? Regex.Replace(emailBody, @"\n\r?", @"<br />") : emailBody);
		}

		private string InterpolateSubmittedValues(string template)
		{
			if (string.IsNullOrEmpty(template))
			{
				return string.Empty;
			}

			// interpolate submitted field values
			var valueFields = AllValueFields();
			return Regex.Replace(template, @"\[.*?\]", match =>
			{
				var field = valueFields.FirstOrDefault(f => string.Format("[{0}]", f.Name).Equals(match.Value, StringComparison.InvariantCultureIgnoreCase));
				return field != null && field.HasSubmittedValue
					? field.SubmittedValueForEmail()
					: string.Empty;
			});
		}

		private void SetFormSubmittedCookie(IPublishedContent content)
		{
			var cookieValue = (Request.Cookies.AllKeys.Contains(FormSubmittedCookieKey) ? Request.Cookies[FormSubmittedCookieKey].Value : null) ?? string.Empty;
			var containsCurrentContent = cookieValue.Contains(FormSubmittedCookieValue(content));

			if (DisallowMultipleSubmissionsPerUser == false)
			{
				if (containsCurrentContent)
				{
					// "only one submission per user" must've been enabled for this form at some point - explicitly remove the content ID from the cookie
					cookieValue = cookieValue.Replace(FormSubmittedCookieValue(content), ",");
					if (cookieValue == ",")
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
			if (containsCurrentContent == false)
			{
				cookieValue = string.Format("{0}{1}", cookieValue.TrimEnd(','), FormSubmittedCookieValue(content));
			}
			Response.Cookies.Add(new HttpCookie(FormSubmittedCookieKey, cookieValue) { Expires = DateTime.Today.AddDays(30) });
		}

		private static string FormSubmittedCookieValue(IPublishedContent content)
		{
			return string.Format(",{0},", content.Id);
		}

		#endregion

		#region Get submitted values

		internal IEnumerable<Data.Row> ExtractSubmittedValues(Result result, IEnumerable<FieldWithValue> fields, Func<FieldWithValue, string, Storage.Row, string> valueFormatter)
		{
			return result.Rows.Select(r =>
				new Data.Row
				{
					Id = r.Id,
					CreatedDate = r.CreatedDate,
					Fields = fields.Select(f =>
						ToDataField(valueFormatter, f, r)
					)
				}
			);
		}

		private static Data.Field ToDataField(Func<FieldWithValue, string, Storage.Row, string> valueFormatter, FieldWithValue field, Storage.Row row)
		{
			return new Data.Field
			{
				Name = field.Name,
				FormSafeName = field.FormSafeName,
				Type = field.Type,
				Value = valueFormatter != null ? valueFormatter(field, row.Fields.ContainsKey(field.FormSafeName) ? row.Fields[field.FormSafeName] : null, row) : null
			};
		}

		#endregion
	}
}
