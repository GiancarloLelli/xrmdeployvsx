using Avanade.XRM.Deployer.Model;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Avanade.XRM.Deployer.Common
{
	public class Bootstrapper
	{
		public static DeployConfiguration Init(int index)
		{
			var configFile = XDocument.Load("C:\\Config.xml");

			var configurations = configFile.Elements("Configurations").Elements("Configuration").Select(c => new DeployConfiguration
			{
				TFS = new Uri(c.Element("TFS").Value, UriKind.Absolute),
				CRMKey = c.Element("CRMKey").Value,
				Domain = c.Element("Domain").Value,
				Solution = c.Element("Solution").Value,
				Prefix = c.Element("Prefix").Value,
				User = c.Element("User").Value,
				Password = c.Element("Password").Value,
				Workspace = c.Element("Workspace").Value
			});

			return configurations.ElementAt(index);
		}
	}
}
