using Avanade.XRM.Deployer.Service;
using System;
using Xrm.Deploy.Core.Models;
using XRM.Deploy.Core.Managers;
using XRM.Deploy.Core.Providers;

namespace XRM.Deploy.Core
{
    public class PublishOrchestrator
    {
        public event EventHandler<string> ReportProgress;

        public void Publish(DeployConfigurationModel deployConfiguration)
        {
            try
            {
                WebResourceManager container;
                Action<string> reportAction = (m) => { ReportProgress?.Invoke(this, m); };
                var context = new XrmService(deployConfiguration.CRMConnectionString, reportAction);
                var userName = !string.IsNullOrEmpty(deployConfiguration.User) ? deployConfiguration.User : Environment.UserName;
                var workspaceName = !string.IsNullOrEmpty(deployConfiguration.Workspace) ? deployConfiguration.Workspace : Environment.MachineName;
                var sourceControl = new SourceControlManager(workspaceName, userName, reportAction);
                var sourceControlResult = sourceControl.InitializeWorkspace(new Uri(deployConfiguration.TFSCollectionUrl, UriKind.Absolute), CredentialsProvider.GetCredentials(deployConfiguration), deployConfiguration.CheckInEnabled);

                // Must resolve conflicts
                if (!sourceControlResult.Continue) return;

                container = new WebResourceManager(sourceControlResult.Changes, deployConfiguration.Prefix, context);
                ReportProgress?.Invoke(this, $"[LOG] => Trovate {container.WebResources.Count} Web Resource.");
                ReportProgress?.Invoke(this, $"[STS] => Add: {container.AddedItems} - Edit: {container.EditedItems} - Delete: {container.DeletedItems}");
                ReportProgress?.Invoke(this, $"[LOG] => '{deployConfiguration.Prefix}' utilizzato come root.");
                ReportProgress?.Invoke(this, $"[LOG] => Fetch soluzione '{deployConfiguration.Solution}' da CRM.");
                container.EnsureContinue(deployConfiguration.Solution);

                ReportProgress?.Invoke(this, $"[LOG] => Generazione pool OrganizationRequest & scrittura su CRM.");
                context.Flush(container.BuildRequestList(deployConfiguration.Solution));
            }
            catch (Exception exception)
            {
                ReportProgress?.Invoke(this, $"[EXCEPTION] => {exception.Message}");
            }
        }
    }
}
