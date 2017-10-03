using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;
using FormEditor.Fields;
using FormEditor.Fields.Statistics;
using FormEditor.Storage;
using FormEditor.Storage.Statistics;
using FormEditor.Umbraco;
using FormEditor.Validation.Conditions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace FormEditor.Api
{
	[PluginController("FormEditorApi")]
	public class PropertyEditorController : UmbracoAuthorizedJsonController
	{
		// cache the known field types in memory
		private static List<Field> _fieldTypes;
		// cache the known condition types in memory
		private static List<Condition> _conditionTypes;

		public object GetAllFieldTypes()
		{
			try
			{
				if (_fieldTypes == null)
				{
					_fieldTypes = GetInstancesOf<Field>(typeof(CustomField), typeof(CustomFieldFixedValues));

					// add any defined custom fields
					if (FormEditor.Configuration.Instance.CustomFields.Any())
					{
						_fieldTypes.AddRange(
							FormEditor.Configuration.Instance.CustomFields.Select(c =>
								c.FixedValues 
									? (Field) new CustomFieldFixedValues(c.Type, c.Name) 
									: new CustomField(c.Type, c.Name)
								)
							);
					}
				}
				_fieldTypes.RemoveAll(f => f.CanBeAddedToForm == false);

				var jsonFields = JArray.FromObject(_fieldTypes, new JsonSerializer
				{
					TypeNameHandling = SerializationHelper.TypeNameHandling,
					ContractResolver = SerializationHelper.ContractResolver
				});
				for (var i = 0; i < _fieldTypes.Count; i++)
				{
					jsonFields[i]["isValueField"] = _fieldTypes[i] is FieldWithValue;
				}

				var json = jsonFields.ToString();
				json = SerializationHelper.FormatJson(json);

				var resp = new HttpResponseMessage
				{
					Content = new StringContent(json, Encoding.UTF8, "application/json")
				};

				return resp;
			}
			catch (Exception ex)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
			}
		}

		public object GetAllConditionTypes()
		{
			try
			{
				if (_conditionTypes == null)
				{
					_conditionTypes = GetInstancesOf<Condition>(typeof(CustomCondition));

					// add any defined custom fields
					if(FormEditor.Configuration.Instance.CustomConditions.Any())
					{
						_conditionTypes.AddRange(
							FormEditor.Configuration.Instance.CustomConditions.Select(c =>
								new CustomCondition(c.Type, c.Name)
							)
						);
					}

				}

				var json = JsonConvert.SerializeObject(_conditionTypes, SerializationHelper.SerializerSettings);
				json = SerializationHelper.FormatJson(json);

				var resp = new HttpResponseMessage
				{
					Content = new StringContent(json, Encoding.UTF8, "application/json")
				};

				return resp;				
			}
			catch (Exception ex)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
			}
		}

		private static List<T> GetInstancesOf<T>(params Type[] ignoredTypes) where T : class
		{
			var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
				{
					// #42 - for some reason this has a tendency to fail on UaaS. we'll try/catch it for now and maybe handle it better down the road.
					try
					{
						return a.GetTypes().Where(t =>
							// no abstract types
							t.IsAbstract == false
							// must be type of T
							&& typeof(T).IsAssignableFrom(t)
							// must have a parameterless constructor
							&& t.GetConstructor(Type.EmptyTypes) != null
							// not in the ignored list of types
							&& ignoredTypes.Contains(t) == false
						);
					}
					catch
					{
						// ignore for now
						return new Type[] {};
					}
				}
			).ToList();

			return types.Select(t => Activator.CreateInstance(t) as T).ToList();
		}

		public IEnumerable<string> GetEmailTemplates()
		{
			return EmailRenderer.GetAllEmailTemplates();
		}

		public IEnumerable<string> GetRowIcons()
		{
			// get all PNG icons in /app_plugins/formeditor/editor/rows/
			var directory = new DirectoryInfo(HostingEnvironment.MapPath("~/app_plugins/formeditor/editor/rows/"));
			if(directory.Exists == false)
			{
				return new string[] { };
			}
			return directory.GetFiles("*.png").Select(f => f.Name);
		}

		public void RemoveData(int id, [FromBody]IEnumerable<Guid> ids)
		{
			var index = IndexHelper.GetIndex(id);
			index.Remove(ids);
		}

		public object GetData(int id, int page, string sortField, bool sortDescending, string searchQuery = null)
		{

			// NOTE: this is fine for now, but eventually make it should probably be configurable
			const int PerPage = 10;

			var document = ContentHelper.GetById(id);
			if (document == null)
			{
				return null;
			}
			var model = ContentHelper.GetFormModel(document);
			if (model == null)
			{
				return null;
			}

			var preValues = ContentHelper.GetPreValues(document, FormModel.PropertyEditorAlias);
			var allFields = GetAllFieldsForDisplay(model, document, preValues);
			var statisticsEnabled = ContentHelper.StatisticsEnabled(preValues);
			var approvalEnabled = ContentHelper.ApprovalEnabled(preValues);

			var index = IndexHelper.GetIndex(id);
			var fullTextIndex = index as IFullTextIndex;
			var result = (fullTextIndex != null && string.IsNullOrWhiteSpace(searchQuery) == false
					? fullTextIndex.Search(searchQuery, allFields.Select(f => f.FormSafeName).ToArray(), sortField, sortDescending, PerPage, (page - 1) * PerPage)
					: index.Get(sortField, sortDescending, PerPage, (page - 1) * PerPage) 
				) ?? Result.Empty(sortField, sortDescending);
			var totalPages = (int)Math.Ceiling((double)result.TotalRows / PerPage);

			// out of bounds request - e.g. right after removing some rows?
			if (page > totalPages && totalPages > 0)
			{
				// repeat the query but get the last page
				page = totalPages;
				result = index.Get(sortField, sortDescending, PerPage, (page - 1) * PerPage);
			}

			var rows = model.ExtractSubmittedValues(result, allFields, (field, value, row) => field.FormatValueForDataView(value, document, row.Id));

			return new
			{
				fields = allFields.Select(f => new { name = f.Name, sortName = f.FormSafeName }).ToArray(),
				rows = rows.Select(r => new
				{
					_id = r.Id,
					_createdDate = r.CreatedDate,
					_approval = r.ApprovalState.ToString().ToLowerInvariant(),
					values = r.Fields.Select(f => f.Value)
				}).ToArray(),
				currentPage = page,
				totalPages = totalPages,
				sortField = result.SortField,
				sortDescending = result.SortDescending,
				supportsSearch = fullTextIndex != null,
				supportsStatistics = statisticsEnabled && index is IStatisticsIndex && allFields.StatisticsFields().Any(),
				supportsApproval = approvalEnabled && index is IApprovalIndex
			};
		}

		public object GetMediaUrl(int id)
		{
			var imageField = new ImageField { MediaId = id };
			var image = imageField.Media;
			return image != null ? new { id = image.Id, url = image.Url } : null;
		}

		public object GetFieldValueFrequencyStatistics(int id)
		{
			var document = ContentHelper.GetById(id);
			if(document == null)
			{
				return null;
			}
			var model = ContentHelper.GetFormModel(document);
			if(model == null)
			{
				return null;
			}

			var statisticsFields = model.AllValueFields().OfType<IValueFrequencyStatisticsField>().ToList();
			if(statisticsFields.Any() == false)
			{
				return null;
			}

			var index = IndexHelper.GetIndex(id) as IStatisticsIndex;
			if(index == null)
			{
				return null;
			}

			var fieldValueFrequencyStatistics = index.GetFieldValueFrequencyStatistics(statisticsFields.StatisticsFieldNames());

			return new
			{
				totalRows = fieldValueFrequencyStatistics.TotalRows,
				fields = fieldValueFrequencyStatistics.FieldValueFrequencies
					.Where(f => statisticsFields.Any(v => v.FormSafeName == f.Field))
					.Select(f =>
					{
						var field = statisticsFields.First(v => v.FormSafeName == f.Field);
						return new
						{
							name = field.Name,
							formSafeName = field.FormSafeName,
							multipleValuesPerEntry = field.MultipleValuesPerEntry,
							values = f.Frequencies.Select(v => new
							{
								value = v.Value,
								frequency = v.Frequency
							})
						};
					})
			};
		}

		[HttpPut]
		public object SetApprovalState(int id, SetApprovalStateRequest request)
		{
			var index = IndexHelper.GetIndex(id) as IApprovalIndex;
			if(index != null && index.SetApprovalState(request.ApprovalState, request.RowId))
			{
				return new
				{
					newApprovalState = request.ApprovalState.ToString().ToLowerInvariant()
				};
			}
			return null;
		}

		internal static List<FieldWithValue> GetAllFieldsForDisplay(FormModel model, IContent document, IDictionary<string, PreValue> preValues = null)
		{
			var allFields = model.AllValueFields().ToList();

			preValues = preValues ?? ContentHelper.GetPreValues(document, FormModel.PropertyEditorAlias);

			// show logged IPs?
			if (ContentHelper.IpDisplayEnabled(preValues) && ContentHelper.IpLoggingEnabled(preValues))
			{
				// IPs are being logged, add a single line text field to retrieve IPs as a string
				allFields.Add(new TextBoxField { Name = "IP", FormSafeName = "_ip" });
			}
			return allFields;
		}

		public class SetApprovalStateRequest
		{
			public ApprovalState ApprovalState { get; set; }

			public Guid RowId { get; set; }
		}
	}
}
