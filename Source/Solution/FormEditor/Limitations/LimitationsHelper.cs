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
					var handler = Activator.CreateInstance(Configuration.Instance.MaxSubmissionsForCurrentUserHandlerType) as IMaxSubmissionsForCurrentUserHandler;
					if(handler == null)
					{
						throw new ConfigurationErrorsException(string.Format("Activator was unable to instantiate the custom MaxSubmissionsForCurrentUserHandler type \"{0}\"", Configuration.Instance.MaxSubmissionsForCurrentUserHandlerType.AssemblyQualifiedName));
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
