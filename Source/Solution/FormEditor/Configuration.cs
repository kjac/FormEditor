using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Xml.Linq;
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
			XDocument configXml = null;
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
			var customFields = configXml.Root.Element("CustomFields");
			if (customFields != null)
			{
				CustomFields.AddRange(
					customFields.Elements("Field").Where(e => e.Attribute("type") != null).Select(e =>
						new CustomField
						{
							Name = e.Attribute("name") != null ? e.Attribute("name").Value : "No name",
							Type = e.Attribute("type").Value,
							FixedOptions = e.Attribute("fixedOptions") != null && e.Attribute("fixedOptions").Value == "true"
						}
					)
				);
			}
			var storage = configXml.Root.Element("Storage");
			if (storage != null)
			{
				var index = storage.Element("Index");
				if (index != null)
				{
					var indexTypeAttribute = index.Attribute("type");
					if (indexTypeAttribute != null && string.IsNullOrEmpty(indexTypeAttribute.Value) == false)
					{
						try
						{
							var indexType = Type.GetType(indexTypeAttribute.Value);
							if (indexType == null)
							{
								throw new ConfigurationErrorsException(string.Format("Custom Index type \"{0}\" could not be found", indexTypeAttribute.Value));
							}
							if (indexType.GetInterfaces().Contains(typeof(IIndex)) == false)
							{
								throw new ConfigurationErrorsException(string.Format("Custom Index type \"{0}\" does not implement IIndex interface", indexTypeAttribute.Value));
							}
							// make sure the type has a constructor that takes an integer (content ID) as it's only parameter
							if (indexType.GetConstructors().Any(c =>
							{
								var parameters = c.GetParameters();
								return parameters.Count() == 1 && parameters.First().ParameterType == typeof(int);
							}) == false)
							{
								throw new ConfigurationErrorsException(string.Format("Custom Index type \"{0}\" does not contain a constructor that takes an integer (content ID) as it's only parameter", indexTypeAttribute.Value));
							}
							IndexType = indexType;
						}
						catch (Exception ex)
						{
							Log.Error(ex, "Could not load custom Index type");
						}
					}
				}
			}
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

		public List<CustomField> CustomFields { get; private set; }

		public Type IndexType { get; private set; }

		public class CustomField
		{
			public string Type { get; set; }
			public string Name { get; set; }
			public bool FixedOptions { get; set; }
		}
	}
}
