using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Web;
using FormEditor.Data;
using FormEditor.Events;
using FormEditor.Fields;
using FormEditor.Storage;
using Umbraco.Core.Models;
using Umbraco.Web;
using Field = FormEditor.Fields.Field;

namespace FormEditor
{
	public class FormModel
	{
		private IEnumerable<Page> _pages;
		private IEnumerable<Row> _rows;
		public const string PropertyEditorAlias = @"FormEditor.Form";

		// properties configured by the editor
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

		[Obsolete("Use Pages instead of Rows. This will be removed before v1.0.")]
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
		public string EmailNotificationRecipients { get; set; }
		public string EmailNotificationSubject { get; set; }
		public string EmailNotificationFromAddress { get; set; }
		public string EmailConfirmationRecipientsField { get; set; }
		public string EmailConfirmationSubject { get; set; }
		public string EmailConfirmationFromAddress { get; set; }
		public int SuccessPageId { get; set; }

		// properties configured in prevalues
		private string EmailNotificationTemplate { get; set; }
		private string EmailConfirmationTemplate { get; set; }
		private bool LogIp { get; set; }
		private bool StripHtml { get; set; }
		private bool DisableValidation { get; set; }

		// events
		public static event FormEditorCancelEventHandler BeforeAddToIndex;
		public static event FormEditorEventHandler AfterAddToIndex;

		public bool CollectSubmittedValues(bool redirect = true)
		{
			if (UmbracoContext.Current == null || UmbracoContext.Current.PublishedContentRequest == null || UmbracoContext.Current.PublishedContentRequest.PublishedContent == null)
			{
				return false;
			}
			return CollectSubmittedValues(UmbracoContext.Current.PublishedContentRequest.PublishedContent, redirect);
		}

		public bool CollectSubmittedValues(IPublishedContent content, bool redirect = true)
		{
			// currently not supporting GET forms ... will require some limitation on fields and stuff
			if (HttpContext.Current.Request.HttpMethod != "POST")
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
			var beforeAddToIndexErrorMessage = RaiseBeforeAddToIndex();
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

			// tell everyone that something was added
			RaiseAfterAddToIndex(rowId);

			SendEmails(content, valueFields);

			if (redirect && SuccessPageId > 0)
			{
				RedirectToSuccesPage();
			}

			return true;
		}

		public FormData GetSubmittedValues(int page = 1, int perPage = 10, string sortField = null, bool sortDescending = false)
		{
			if(UmbracoContext.Current == null || UmbracoContext.Current.PublishedContentRequest == null || UmbracoContext.Current.PublishedContentRequest.PublishedContent == null)
			{
				return new FormData();
			}
			return GetSubmittedValues(UmbracoContext.Current.PublishedContentRequest.PublishedContent, page, perPage, sortField, sortDescending);
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

		public IEnumerable<Field> AllFields()
		{
			return Pages.SelectMany(p => p.AllFields());
		}

		public IEnumerable<FieldWithValue> AllValueFields()
		{
			return AllFields().OfType<FieldWithValue>();
		}

		#region This is for backwards compatability with v0.10.0.1 - should be removed before releasing v1.0

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

		// this is for backwards compatability with v0.10.0.1 - should be removed at some point
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
			var valueCollection = HttpContext.Current.Request.Form;
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
			var rowId = Guid.NewGuid();

			// extract all index values
			var indexFields = valueFields.ToDictionary(f => f.FormSafeName, f => FormatForIndexAndSanitize(f, content, rowId));

			// add the IP of the user if enabled on the data type
			if(LogIp)
			{
				indexFields.Add("_ip", HttpContext.Current.Request.UserHostAddress);
			}

			// store fields in index
			var index = IndexHelper.GetIndex(content.Id);
			index.Add(indexFields, rowId);

			return rowId;
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

		private string RaiseBeforeAddToIndex()
		{
			if(BeforeAddToIndex != null)
			{
				try
				{
					var cancelEventArgs = new FormEditorCancelEventArgs();
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

		private void RaiseAfterAddToIndex(Guid rowId)
		{
			if(AfterAddToIndex != null)
			{
				try
				{
					AfterAddToIndex.Invoke(this, new FormEditorEventArgs(rowId));
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
				SendEmailType(EmailNotificationSubject, EmailNotificationFromAddress, EmailNotificationRecipients, content, EmailNotificationTemplate, "Notification", ref emailBody);
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
				// can we reuse the rendered email body?
				if(EmailConfirmationTemplate != EmailNotificationTemplate)
				{
					// nope
					emailBody = null;
				}
				SendEmailType(EmailConfirmationSubject, EmailConfirmationFromAddress, recipientsField.SubmittedValue, content, EmailConfirmationTemplate, "Confirmation", ref emailBody);
			}
		}

		private void SendEmailType(string subject, string senderAddress, string recipientAddresses, IPublishedContent currentContent, string template, string emailType, ref string emailBody)
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
			// send emails to the recipients
			SendEmails(subject, emailBody, senderEmailAddress, addresses);
		}
		
		private static void SendEmails(string subject, string body, MailAddress from, IEnumerable<MailAddress> to)
		{
			subject = subject ?? string.Empty;
			if(string.IsNullOrEmpty(body) == false)
			{
				var client = new SmtpClient();
				foreach(var address in to)
				{
					var mail = new MailMessage
					{
						From = from,
						Subject = subject,
						Body = body,
						IsBodyHtml = body.Contains("<html") || body.Contains("<body") || body.Contains("<div")
					};
					mail.To.Add(address);
					try
					{
						client.Send(mail);
					}
					catch(Exception ex)
					{
						Log.Error(ex, "Email could not be sent, see exception for details.");
					}
				}
			}
		}

		private IEnumerable<MailAddress> ParseEmailAddresses(string emails)
		{
			var addresses = new List<MailAddress>();
			foreach(var email in emails.Split(new[] { ',', ' ', ';' }))
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
				var preValues = UmbracoContext.Current.Application.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(property.DataTypeId);
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
			var helper = new UmbracoHelper(UmbracoContext.Current);
			var redirectTo = helper.TypedContent(SuccessPageId);
			if(redirectTo != null)
			{
				HttpContext.Current.Response.Redirect(redirectTo.Url);
			}
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
