using Microsoft.Xrm.Sdk;

namespace Avanade.XRM.Deployer.Extensions
{
	public static class EntityExtension
	{
		public static void SetIfNotNull(this OrganizationRequest req, string attribute, object data)
		{
			if (req != null)
			{
				req[attribute] = data;
			}
		}
	}
}
