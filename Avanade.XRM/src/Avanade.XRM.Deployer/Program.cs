using Avanade.XRM.Deployer.Common;
using Avanade.XRM.Deployer.Extensions;
using Avanade.XRM.Deployer.Model;
using Avanade.XRM.Deployer.Service;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Avanade.XRM.Deployer
{
	class Program
	{
		static void Main(string[] args)
		{
			var uri = new Uri(ConfigurationProvider.GetConnectionString("TFS"), UriKind.Absolute);
			var user = ConfigurationProvider.GetAppSettingsValue("User");
			var pwd = ConfigurationProvider.GetAppSettingsValue("Password");
			var domain = ConfigurationProvider.GetAppSettingsValue("Domain");

			var credentials = new TfsClientCredentials(new WindowsCredential(new NetworkCredential(user, pwd, domain)));
			List<CrmWebResource> resources = new List<CrmWebResource>();

			using (TfsTeamProjectCollection collection = new TfsTeamProjectCollection(uri, credentials))
			{
				Console.WriteLine($"[LOG] => Enumerazione cambiamenti sul Workspace.");
				VersionControlServer versionControl = collection.GetService(typeof(VersionControlServer)) as VersionControlServer;
				Workspace workspace = versionControl.GetWorkspace(Environment.MachineName, Environment.UserName);
				PendingChange[] changes = workspace.GetPendingChanges();
				Console.WriteLine($"[LOG] => Trovate {changes.Count()} Check-Out items.");

				foreach (var change in changes)
				{
					var crmWebResource = new CrmWebResource
					{
						ChangeType = change.ChangeTypeName,
						File = change.LocalItem,
						DisplayName = change.FileName
					};

					if (crmWebResource.FileType != 99)
						resources.Add(crmWebResource);
				}

				workspace.CheckIn(changes, $"CI Checkin by {user}");
				Console.WriteLine($"[LOG] => Check-In effettuato on behalf of {user}.");
			}

			var add = resources.Count(p => p.ChangeType.ToLower().Equals("add"));
			var edit = resources.Count(p => p.ChangeType.ToLower().Equals("edit"));
			var delete = resources.Count(p => p.ChangeType.ToLower().Equals("delete"));

			Console.WriteLine($"\n[LOG] => Trovate {resources.Count} Web Resource.");
			Console.WriteLine($"[STS] => Add: {add} - Edit: {edit} - Delete: {delete}\n");

			var context = new XrmWrapper("CRM");
			var prefix = ConfigurationProvider.GetAppSettingsValue("Prefix");
			Console.WriteLine($"[LOG] => '{prefix}' utilizzato come root.");

			var solutionName = ConfigurationProvider.GetAppSettingsValue("SolutionName");
			Console.WriteLine($"[LOG] => Fetch soluzione '{solutionName}' da CRM.");
			var solution = context.GetSolutionByName(solutionName);
			if (solution.Id == Guid.Empty) return; // No solution. Exit.

			Console.WriteLine($"\n[LOG] => Generazione pool OrganizationRequest.");
			List<OrganizationRequest> requests = new List<OrganizationRequest>();
			foreach (var resource in resources)
			{
				var index = resource.File.IndexOf(prefix);
				if (index == -1) continue; // File not valid for this prefix

				var resourceFullName = resource.File.Substring(index).Replace("\\", "/");
				var deleting = resource.ChangeType.ToLower().Equals("delete");
				Entity webRes = null;

				if (!deleting)
				{
					webRes = new Entity("webresource");
					webRes["content"] = resource.FileStream;
					webRes["displayname"] = resourceFullName;
					webRes["description"] = resourceFullName;
					webRes["name"] = resourceFullName;
					webRes["webresourcetype"] = new OptionSetValue(resource.FileType);
				}

				var factory = context.RequestFactory(resource.ChangeType.ToLower(), resourceFullName, webRes, solutionName);
				OrganizationRequest request = factory.Item1;
				OrganizationRequest publishRequest = factory.Item2;

				request.SetIfNotNull("SolutionUniqueName", solutionName);
				requests.AddIfNotNull(request);
				requests.AddIfNotNull(publishRequest);
			}

			Console.WriteLine($"[LOG] => Scrittura su CRM.");
			context.Flush(requests);
		}
	}
}
