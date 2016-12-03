using Avanade.XRM.Deployer.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XRM.Deploy.Core.Managers;
using XRM.Deploy.Core.Models;
using XRM.Deploy.Core.Providers;

namespace XRM.Deploy.Core
{
	public class PublishOrchestrator
	{
		public event EventHandler<string> ReportProgress;

		public void Publish(int configurationIndex)
		{
			try
			{
				WebResourceManager container;
				Action<string> reportAction = (m) => { ReportProgress?.Invoke(this, m); };
				var bootstrap = PublishSettingsProvider.Init(configurationIndex);
				var context = new XrmService(bootstrap.CRMKey, reportAction);
				var userName = !string.IsNullOrEmpty(bootstrap.User) ? bootstrap.User : Environment.UserName;
				var workspaceName = !string.IsNullOrEmpty(bootstrap.Workspace) ? bootstrap.Workspace : Environment.MachineName;
				var sourceControl = new SourceControlManager(workspaceName, userName, reportAction);
				var sourceControlResult = sourceControl.InitializeWorkspace(bootstrap.TFS, CredentialsProvider.GetCredentials(bootstrap));

				// Must resolve conflicts
				if (!sourceControlResult.Continue) return;

				container = new WebResourceManager(sourceControlResult.Changes, bootstrap.Prefix, context);
				ReportProgress?.Invoke(this, $"[LOG] => Trovate {container.WebResources.Count} Web Resource.");
				ReportProgress?.Invoke(this, $"[STS] => Add: {container.AddedItems} - Edit: {container.EditedItems} - Delete: {container.DeletedItems}");
				ReportProgress?.Invoke(this, $"[LOG] => '{bootstrap.Prefix}' utilizzato come root.");
				ReportProgress?.Invoke(this, $"[LOG] => Fetch soluzione '{bootstrap.Solution}' da CRM.");
				container.EnsureContinue(bootstrap.Solution);

				ReportProgress?.Invoke(this, $"[LOG] => Generazione pool OrganizationRequest & scrittura su CRM.");
				context.Flush(container.BuildRequestList(bootstrap.Solution));
			}
			catch (Exception exception)
			{
				ReportProgress?.Invoke(this, $"[EXCEPTION] => {exception.Message}");
			}
		}
	}
}
