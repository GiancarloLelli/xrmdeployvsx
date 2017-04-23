using Avanade.XRM.Deployer.Service;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Linq;
using XRM.Deploy.Core.Fallback;
using XRM.Deploy.Core.Managers;
using XRM.Deploy.Core.Models;
using XRM.Deploy.Core.Providers;
using XRM.Telemetry;
using XRM.Telemetry.Models;

namespace XRM.Deploy.Core
{
    public class PublishOrchestrator
    {
        public event EventHandler<string> ReportProgress;

        public void Publish(DeployConfigurationModel deployConfiguration, TelemetryWrapper telemetry, string singleResourceName, string project)
        {
            try
            {
                Action<string> reportAction = (m) => { ReportProgress?.Invoke(this, m); };
                var tfsCollectionUri = new Uri(deployConfiguration.SourceControlSettings.TFSCollectionUrl, UriKind.Absolute);
                var tfsClientCredentials = CredentialsProvider.GetCredentials(deployConfiguration);
                var context = new XrmService(deployConfiguration.CrmSettings, deployConfiguration.Solution, telemetry, reportAction);

                var userName = !string.IsNullOrEmpty(deployConfiguration.SourceControlSettings.User) ? deployConfiguration.SourceControlSettings.User : Environment.UserName;
                var workspaceName = !string.IsNullOrEmpty(deployConfiguration.Workspace) ? deployConfiguration.Workspace : Environment.MachineName;
                var sourceControl = new SourceControlManager(workspaceName, userName, tfsCollectionUri, tfsClientCredentials, reportAction, telemetry);
                var sourceControlResult = sourceControl.InitializeWorkspace();

                // Must resolve conflicts or something went wrong with TFS interaction
                if (!sourceControlResult.Continue && sourceControlResult.Changes != null) return;

                var changeList = sourceControlResult.Changes;
                if (!string.IsNullOrEmpty(singleResourceName))
                {
                    var filteredChangeList = sourceControlResult.Changes.Where(x => x.FileName.Equals(singleResourceName)).ToArray();
                    changeList = filteredChangeList;
                }

                PublishImpl(context, sourceControl, deployConfiguration, telemetry, changeList, project);
            }
            catch (Exception exception)
            {
                ReportProgress?.Invoke(this, $"[EXCEPTION] => {exception.Message}\n");
                if (!(exception is ToolkitException))
                    telemetry.TrackExceptionWithCustomMetrics(exception);
            }
        }

        private void PublishImpl(XrmService context, SourceControlManager sourceControl, DeployConfigurationModel deployConfiguration, TelemetryWrapper telemetry, PendingChange[] changes, string project)
        {
            try
            {
                WebResourceManager container = new WebResourceManager(changes, deployConfiguration.Prefix, context);
                ReportProgress?.Invoke(this, $"[TFS] => Add: {container.AddedItems} - Edit: {container.EditedItems} - Delete: {container.DeletedItems}");

                if (container.WebResources.Count > 0)
                {
                    ReportProgress?.Invoke(this, $"[CRM] => Found {container.WebResources.Count} Web Resource.");
                    ReportProgress?.Invoke(this, $"[CRM] => '{deployConfiguration.Prefix}' used as base path.");
                    ReportProgress?.Invoke(this, $"[CRM] => Fetching '{deployConfiguration.Solution}' solution from CRM.");
                    container.EnsureContinue(deployConfiguration.Solution, deployConfiguration.Prefix);

                    ReportProgress?.Invoke(this, $"[CRM] => Writing changes to the CRM.");
                    var faultedFlushResult = context.Flush(container.BuildRequestList(deployConfiguration.Solution));
                    ReportProgress?.Invoke(this, $"[CRM] => Writing completed.");

                    if (!faultedFlushResult && deployConfiguration.CheckInEnabled)
                    {
                        ReportProgress?.Invoke(this, $"[TFS] => Checking in changes.");
                        sourceControl.CheckInChanges();
                    }
                }

                telemetry.TrackCustomEventWithCustomMetrics("Deploy Finished", new MetricData("Project Name", project));
            }
            catch (Exception exception)
            {
                ReportProgress?.Invoke(this, $"[EXCEPTION] => {exception.Message}\n");
                if (!(exception is ToolkitException))
                    telemetry.TrackExceptionWithCustomMetrics(exception);
            }
        }
    }
}
