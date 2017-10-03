using System;
using System.Configuration;

namespace FormEditor.Limitations
{
	public class LimitationsHelper
	{
		public static IMaxSubmissionsForCurrentUserHandler GetMaxSubmissionsForCurrentUserHandler()
		{
			if(Configuration.Instance.MaxSubmissionsForCurrentUserHandlerType != null)
			{
				try
				{
					if(!(Activator.CreateInstance(Configuration.Instance.MaxSubmissionsForCurrentUserHandlerType) is IMaxSubmissionsForCurrentUserHandler handler))
					{
						throw new ConfigurationErrorsException($"Activator was unable to instantiate the custom MaxSubmissionsForCurrentUserHandler type \"{Configuration.Instance.MaxSubmissionsForCurrentUserHandlerType.AssemblyQualifiedName}\"");
					}
					return handler;
				}
				catch(Exception ex)
				{
					Log.Error(ex, "Could not create an instance of the custom MaxSubmissionsForCurrentUserHandler type");
				}
			}
			// revert to default handler
			return new MaxSubmissionsForCurrentUserHandler();
		}

	}
}
