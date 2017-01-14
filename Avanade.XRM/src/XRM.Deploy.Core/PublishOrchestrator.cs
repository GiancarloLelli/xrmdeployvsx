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

        public void Publish(DeployConfigurationModel deployConfiguration, TelemetryWrapper telemetry)
        {
            try
            {
                WebResourceManager container;
                Action<string> reportAction = (m) => { ReportProgress?.Invoke(this, m); };
                var context = new XrmService(deployConfiguration.CRMConnectionString, telemetry, reportAction);
                var userName = !string.IsNullOrEmpty(deployConfiguration.User) ? deployConfiguration.User : Environment.UserName;
                var workspaceName = !string.IsNullOrEmpty(deployConfiguration.Workspace) ? deployConfiguration.Workspace : Environment.MachineName;
                var sourceControl = new SourceControlManager(workspaceName, userName, reportAction, telemetry);
                var tfsCollectionUri = new Uri(deployConfiguration.TFSCollectionUrl, UriKind.Absolute);
                var tfsClientCredentials = CredentialsProvider.GetCredentials(deployConfiguration);
                var sourceControlResult = sourceControl.InitializeWorkspace(tfsCollectionUri, tfsClientCredentials, deployConfiguration.CheckInEnabled);

                // Must resolve conflicts
                if (!sourceControlResult.Continue) return;

                container = new WebResourceManager(sourceControlResult.Changes, deployConfiguration.Prefix, context);
                ReportProgress?.Invoke(this, $"[TFS] => Add: {container.AddedItems} - Edit: {container.EditedItems} - Delete: {container.DeletedItems}");

                if (container.WebResources.Count > 0)
                {
                    ReportProgress?.Invoke(this, $"[CRM] => Trovate {container.WebResources.Count} Web Resource.");
                    ReportProgress?.Invoke(this, $"[CRM] => '{deployConfiguration.Prefix}' utilizzato come root.");
                    ReportProgress?.Invoke(this, $"[CRM] => Fetch soluzione '{deployConfiguration.Solution}' da CRM.");
                    container.EnsureContinue(deployConfiguration.Solution);

                    ReportProgress?.Invoke(this, $"[CRM] => Generazione pool OrganizationRequest & scrittura su CRM.");
                    context.Flush(container.BuildRequestList(deployConfiguration.Solution));
                    ReportProgress?.Invoke(this, $"[CRM] => Scrittura su CRM completata.\n");
                }
            }
            catch (Exception exception)
            {
                ReportProgress?.Invoke(this, $"[EXCEPTION] => {exception.Message}\n");
                if (!(exception is DeployException))
                    telemetry.Instance.TrackExceptionWithCustomMetrics(exception);
            }
        }
    }
}
