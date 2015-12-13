using System;
using System.Configuration;

namespace FormEditor.Storage
{
	public class IndexHelper
	{
		public static IIndex GetIndex(int contentId)
		{
			if (Configuration.Instance.IndexType != null)
			{
				// Ninject, go home :-)
				try
				{
					var index = Activator.CreateInstance(Configuration.Instance.IndexType, contentId) as IIndex;
					if (index == null)
					{
						throw new ConfigurationErrorsException(string.Format("Activator was unable to instantiate the custom Index type \"{0}\"", Configuration.Instance.IndexType.AssemblyQualifiedName));
					}
					return index;
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Could not create an instance of the custom Index type");
				}
			}
			// revert to default index
			return new Index(contentId);
		}
	}
}
