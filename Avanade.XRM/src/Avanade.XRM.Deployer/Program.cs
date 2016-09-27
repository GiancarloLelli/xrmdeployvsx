using Avanade.XRM.Deployer.Auth;
using Avanade.XRM.Deployer.Common;
using Avanade.XRM.Deployer.CRM;
using Avanade.XRM.Deployer.Extensions;
using Avanade.XRM.Deployer.Service;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avanade.XRM.Deployer
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0) throw new ArgumentException("Indice di configurazione obbligatorio");
			TimedRunner.RunAndTime(() => TimedMain(args), 3000);
		}

		static void TimedMain(string[] args)
		{
			try
			{
				WebResourceContainer container;
				var bootstrap = Bootstrapper.Init(int.Parse(args[0]));
				var context = new XrmWrapper(bootstrap.CRMKey);
				var userName = !string.IsNullOrEmpty(bootstrap.User) ? bootstrap.User : Environment.UserName;
				var workspaceName = !string.IsNullOrEmpty(bootstrap.Workspace) ? bootstrap.Workspace : Environment.MachineName;

				using (TfsTeamProjectCollection collection = new TfsTeamProjectCollection(bootstrap.TFS, AuthHelper.GetCredentials(bootstrap)))
				{
					Console.WriteLine($"[LOG] => Enumerazione cambiamenti sul Workspace.");
					VersionControlServer versionControl = collection.GetService(typeof(VersionControlServer)) as VersionControlServer;
					Workspace workspace = versionControl.GetWorkspace(workspaceName, userName);

					PendingChange[] changes = workspace.GetPendingChanges();
					container = new WebResourceContainer(changes, bootstrap.Prefix);
					Console.WriteLine($"[LOG] => Trovate {container.TotalChanges} Check-Out items.");

					var folderFilters = changes.Select(c => c.LocalOrServerFolder).Distinct().ToArray();
					if (folderFilters.Length > 0)
					{
						var conflicts = workspace.QueryConflicts(folderFilters, true);
						if (conflicts.Length > 0)
						{
							Console.WriteLine($"[LOG] => Rilevati {conflicts.Length} conflitti. Aggiorna il workspace prima di continuare");
							return;
						}
					}

					if (changes.Length > 0)
					{
						workspace.CheckIn(changes, $"CI Checkin by {Environment.UserName}");
						Console.WriteLine($"[LOG] => Check-In effettuato on behalf of {Environment.UserName}.");
					}
				}

				Console.WriteLine($"[LOG] => Trovate {container.WebResources.Count} Web Resource.");
				Console.WriteLine($"[STS] => Add: {container.AddedItems} - Edit: {container.EditedItems} - Delete: {container.DeletedItems}");
				Console.WriteLine($"[LOG] => '{bootstrap.Prefix}' utilizzato come root.");
				Console.WriteLine($"[LOG] => Fetch soluzione '{bootstrap.Solution}' da CRM.");

				if (container.AddedItems > 0)
				{
					var solution = context.GetSolutionByName(bootstrap.Solution);
					if (solution.Id == Guid.Empty)
					{
						throw new Exception($"No solution found in CRM with unique name: '{bootstrap.Solution}'");
					}
				};

				Console.WriteLine($"[LOG] => Generazione pool OrganizationRequest.");
				List<OrganizationRequest> requests = new List<OrganizationRequest>();
				foreach (var resource in container.WebResources)
				{
					if (string.IsNullOrEmpty(resource.FullName)) continue;
					var factory = context.RequestFactory(resource.ChangeType, resource.FullName, resource.ToEntity(), bootstrap.Solution);

					OrganizationRequest request = factory.Item1;
					OrganizationRequest publishRequest = factory.Item2;

					request.SetIfNotNull("SolutionUniqueName", bootstrap.Solution);
					requests.AddIfNotNull(request);
					requests.AddIfNotNull(publishRequest);
				}

				Console.WriteLine($"[LOG] => Scrittura su CRM.");
				context.Flush(requests);
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[EXCEPTION] => {exception.Message}");
			}
		}
	}
}
