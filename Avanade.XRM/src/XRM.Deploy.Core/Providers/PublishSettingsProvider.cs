using System;
using System.Linq;
using System.Xml.Linq;
using Xrm.Deploy.Core.Models;

namespace XRM.Deploy.Core.Providers
{
	internal class PublishSettingsProvider
	{
		internal static DeployConfigurationModel Init(int index)
		{
			string path = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\XrmDeployConfig.xml");
			var configFile = XDocument.Load(path);

			var configurations = configFile.Elements("Configurations")
										.Elements("Configuration")
										.Select(c => new DeployConfigurationModel
										{
											TFS = new Uri(c.Element("TFS").Value, UriKind.Absolute),
											CRMKey = c.Element("CRMKey").Value,
											Domain = c.Element("Domain").Value,
											Solution = c.Element("Solution").Value,
											Prefix = c.Element("Prefix").Value,
											User = c.Element("User").Value,
											Password = c.Element("Password").Value,
											Workspace = c.Element("Workspace").Value,
                                            ConfigurationName = c.Element("ConfigurationName").Value
										});

			return configurations.ElementAt(index);
		}
	}
}
