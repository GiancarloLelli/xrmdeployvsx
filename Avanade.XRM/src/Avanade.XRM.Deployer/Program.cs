using Avanade.XRM.Deployer.Auth;
using Avanade.XRM.Deployer.Common;
using Avanade.XRM.Deployer.CRM;
using Avanade.XRM.Deployer.Service;
using Avanade.XRM.Deployer.TFS;
using System;

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
				var sourceControl = new SourceControlManager(workspaceName, userName);
				var sourceControlResult = sourceControl.InitializeWorkspace(bootstrap.TFS, AuthHelper.GetCredentials(bootstrap));

				// Must resolve conflicts
				if (!sourceControlResult.Continue) return;

				container = new WebResourceContainer(sourceControlResult.Changes, bootstrap.Prefix, context);
				Console.WriteLine($"[LOG] => Trovate {container.WebResources.Count} Web Resource.");
				Console.WriteLine($"[STS] => Add: {container.AddedItems} - Edit: {container.EditedItems} - Delete: {container.DeletedItems}");
				Console.WriteLine($"[LOG] => '{bootstrap.Prefix}' utilizzato come root.");
				Console.WriteLine($"[LOG] => Fetch soluzione '{bootstrap.Solution}' da CRM.");
				container.EnsureContinue(bootstrap.Solution);

				Console.WriteLine($"[LOG] => Generazione pool OrganizationRequest & scrittura su CRM.");
				context.Flush(container.BuildRequestList(bootstrap.Solution));
			}
			catch (Exception exception)
			{
				Console.WriteLine($"[EXCEPTION] => {exception.Message}");
			}
		}
	}
}
