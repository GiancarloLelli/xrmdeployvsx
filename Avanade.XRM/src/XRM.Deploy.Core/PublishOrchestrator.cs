using Avanade.XRM.Deployer.Service;
using System;
using Xrm.Deploy.Core.Models;
using XRM.Deploy.Core.Fallback;
using XRM.Deploy.Core.Managers;
using XRM.Deploy.Core.Providers;
using XRM.Telemetry;

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
                var tfsCollectionUri = new Uri(deployConfiguration.TFSCollectionUrl, UriKind.Absolute);
                var tfsClientCredentials = CredentialsProvider.GetCredentials(deployConfiguration);
                var sourceControlResult = sourceControl.InitializeWorkspace(tfsCollectionUri, tfsClientCredentials, deployConfiguration.CheckInEnabled);

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
                ReportProgress?.Invoke(this, $"[LOG] => Scrittura su CRM completata.\n");
            }
            catch (Exception exception)
            {
                ReportProgress?.Invoke(this, $"[EXCEPTION] => {exception.Message}\n");
                if (!(exception is DeployException))
                    TelemetryWrapper.Instance.TrackExceptionWithCustomMetrics(exception);
            }
        }
    }
}
