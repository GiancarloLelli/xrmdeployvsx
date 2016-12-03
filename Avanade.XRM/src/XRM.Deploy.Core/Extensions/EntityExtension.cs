using Microsoft.Xrm.Sdk;

namespace XRM.Deploy.Core.Extensions
{
	internal static class EntityExtension
	{
		internal static void SetIfNotNull(this OrganizationRequest req, string attribute, object data)
		{
			if (req != null)
			{
				req[attribute] = data;
			}
		}
	}
}
