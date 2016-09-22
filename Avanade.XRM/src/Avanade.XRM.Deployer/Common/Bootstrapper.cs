using Avanade.XRM.Deployer.Model;
using System;

namespace Avanade.XRM.Deployer.Common
{
	public class Bootstrapper
	{
		public static DeployConfiguration Init()
		{
			return new DeployConfiguration
			{
				TFS = new Uri(ConfigurationProvider.GetConnectionString("TFS"), UriKind.Absolute),
				User = ConfigurationProvider.GetAppSettingsValue("User"),
				Password = ConfigurationProvider.GetAppSettingsValue("Password"),
				Domain = ConfigurationProvider.GetAppSettingsValue("Domain"),
				Prefix = ConfigurationProvider.GetAppSettingsValue("Prefix"),
				Solution = ConfigurationProvider.GetAppSettingsValue("SolutionName"),
				CRMKey = ConfigurationProvider.GetAppSettingsValue("CRMKey"),
				Workspace = ConfigurationProvider.GetAppSettingsValue("Workspace")
			};
		}
	}
}
