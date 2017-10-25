using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;
using FormEditor.Limitations;
using FormEditor.Storage;

namespace FormEditor
{
	internal class Configuration
	{
		private static readonly object InstanceLock = new object();
		private static Configuration _instance;

		private Configuration()
		{
			CustomFields = new List<CustomField>();
			CustomConditions = new List<CustomCondition>();
			Load();
		}

		private void Load()
		{
			var path = HostingEnvironment.MapPath(@"/config/formEditor.config");
			if (string.IsNullOrEmpty(path))
			{
				return;
			}
			var configFile = new FileInfo(path);
			if (configFile.Exists == false)
			{
				return;
			}
			XDocument configXml;
			try
			{
				configXml = XDocument.Load(configFile.FullName);
				if (configXml.Root == null)
				{
					throw new ConfigurationErrorsException("Missing root node in configuration file.");
				}
			}
			catch (Exception ex)
			{
				// bad config XML
				Log.Error(ex, "Could not parse the configuration file.");
				return;
			}

			var jobs = configXml.Root.Element("Jobs");
			var authToken = jobs?.Attribute("authToken")?.Value;
			Jobs = string.IsNullOrEmpty(authToken) ? null : new JobsConfiguration(authToken);

			var customFields = configXml.Root.Element("CustomFields");
			if (customFields != null)
			{
				CustomFields.AddRange(
					customFields.Elements("Field").Where(e => e.Attribute("type") != null).Select(e =>
						new CustomField
						{
							Name = e.Attribute("name")?.Value ?? "No name",
							Type = e.Attribute("type").Value,
							FixedValues = e.Attribute("fixedValues")?.Value == "true"
						}
					)
				);
			}

			var customConditions = configXml.Root.Element("CustomConditions");
			if(customConditions != null)
			{
				CustomConditions.AddRange(
					customConditions.Elements("Condition").Where(e => e.Attribute("type") != null).Select(e =>
						new CustomCondition
						{
							Name = e.Attribute("name")?.Value ?? "No name",
							Type = e.Attribute("type").Value
						}
					)
				);
			}

			var index = configXml.Root.Element("Storage")?.Element("Index");
			if (index != null)
			{
				var type = ParseType(index, typeof(IIndex),
					t => t.GetConstructors().Any(c =>
					{
						var parameters = c.GetParameters();
						return parameters.Length == 1 && parameters.First().ParameterType == typeof(int);
					})
						? null
						: $"Custom Index type \"{t}\" does not contain a constructor that takes an integer (content ID) as it's only parameter"
				);
				if(type != null)
				{
					IndexType = type;
				}
			}

			var maxSubmissionsForCurrentUserHandler = configXml.Root.Element("Limitations")?.Element("MaxSubmissionsForCurrentUserHandler");
			if(maxSubmissionsForCurrentUserHandler != null)
			{
				var type = ParseType(maxSubmissionsForCurrentUserHandler, typeof(IMaxSubmissionsForCurrentUserHandler),
					t => t.GetConstructors().Any(c => c.GetParameters().Any() == false) 
						? null
						: $"Custom MaxSubmissionsForCurrentUserHandler type \"{t}\" does not contain a parameterless constructor"
				);

				if(type != null)
				{
					MaxSubmissionsForCurrentUserHandlerType = type;
				}
			}

			var delimiter = configXml.Root.Element("CsvExport")?.Attribute("delimiter")?.Value ?? ";";
			CsvExport = new CsvExportConfiguration(delimiter);
		}

		internal static Configuration Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (InstanceLock)
					{
						if (_instance == null)
						{
							_instance = new Configuration();
						}
					}
				}
				return _instance;
			}
		}

		public JobsConfiguration Jobs { get; private set; }

		public List<CustomField> CustomFields { get; }

		public List<CustomCondition> CustomConditions { get; }

		public Type IndexType { get; private set; }

		public Type MaxSubmissionsForCurrentUserHandlerType { get; private set; }

		public CsvExportConfiguration CsvExport { get; private set; }

		private Type ParseType(XElement element, Type declaration, Func<Type, string> validateType)
		{
			var typeAttribute = element.Attribute("type");
			if(typeAttribute != null && string.IsNullOrEmpty(typeAttribute.Value) == false)
			{
				var name = declaration.Name.TrimStart('I');
				try
				{
					var type = Type.GetType(typeAttribute.Value);
					if(type == null)
					{
						throw new ConfigurationErrorsException($"Custom {name} type \"{typeAttribute.Value}\" could not be found");
					}
					if(type.GetInterfaces().Contains(declaration) == false)
					{
						throw new ConfigurationErrorsException($"Custom {name} type \"{typeAttribute.Value}\" does not implement the {declaration.Name} interface");
					}
					var validationError = validateType(type);
					if(string.IsNullOrEmpty(validationError) == false)
					{
						throw new ConfigurationErrorsException(validationError);						
					}
					return type;
				}
				catch(Exception ex)
				{
					Log.Error(ex, $"Could not load custom {name} type");
				}
			}
			return null;
		}

		public class CustomField
		{
			public string Type { get; set; }
			public string Name { get; set; }
			public bool FixedValues { get; set; }
		}

		public class CustomCondition
		{
			public string Type { get; set; }
			public string Name { get; set; }
		}

		public class CsvExportConfiguration
		{
			public CsvExportConfiguration(string delimiter)
			{
				Delimiter = delimiter;
			}

			public string Delimiter { get; }
		}

		public class JobsConfiguration
		{
			private readonly string _authToken;

			public JobsConfiguration(string authToken)
			{
				_authToken = authToken;
			}

			public bool IsValidAuthToken(string authToken)
			{
				return string.IsNullOrEmpty(_authToken) == false && _authToken == authToken;
			}
		}
	}
}