using CRMDevLabs.Toolkit.AzOps;
using CRMDevLabs.Toolkit.AzOps.Providers;
using CRMDevLabs.Toolkit.Models.Telemetry;
using CRMDevLabs.Toolkit.Models.Xrm;
using CRMDevLabs.Toolkit.Telemetry;
using CRMDevLabs.Toolkit.Xrm;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Linq;

namespace CRMDevLabs.Toolkit.Feature.WebResource
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
                var context = new XrmService(deployConfiguration.DynamicsSettings, deployConfiguration.Solution, telemetry, reportAction);

                var workspaceName = !string.IsNullOrEmpty(deployConfiguration.Workspace) ? deployConfiguration.Workspace : Environment.MachineName;
                var sourceControl = new VersioningService(workspaceName, Environment.UserName, tfsCollectionUri, tfsClientCredentials, reportAction, telemetry);
                var sourceControlResult = sourceControl.InitializeWorkspace();

                // Must resolve conflicts or something went wrong with TFS interaction
                if (!sourceControlResult.Continue && sourceControlResult.Changes == null) return;

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
                ReportProgress?.Invoke(this, $"[ERROR] => {exception.Message}\n");
                if (!(exception is ToolkitException))
                    telemetry.TrackExceptionWithCustomMetrics(exception);
            }
        }

        private void PublishImpl(XrmService context, VersioningService sourceControl, DeployConfigurationModel deployConfiguration, TelemetryWrapper telemetry, PendingChange[] changes, string project)
        {
            try
            {
                ChangeManagerService container = new ChangeManagerService(changes, deployConfiguration.Prefix, context);
                ReportProgress?.Invoke(this, $"[AZOPS] => Add: {container.AddedItems} - Edit: {container.EditedItems} - Delete: {container.DeletedItems}");

                if (container.WebResources.Count > 0)
                {
                    ReportProgress?.Invoke(this, $"[DYNAMICS] => Found {container.WebResources.Count} Web Resource.");
                    ReportProgress?.Invoke(this, $"[DYNAMICS] => '{deployConfiguration.Prefix}' used as base path.");
                    ReportProgress?.Invoke(this, $"[DYNAMICS] => Fetching '{deployConfiguration.Solution}' solution from CRM.");
                    container.EnsureContinue(deployConfiguration.Solution, deployConfiguration.Prefix);

                    ReportProgress?.Invoke(this, $"[DYNAMICS] => Writing changes to the CRM.");
                    var faultedFlushResult = context.Flush(container.BuildRequestList(deployConfiguration.Solution));
                    ReportProgress?.Invoke(this, $"[DYNAMICS] => Writing completed.");

                    if (!faultedFlushResult && deployConfiguration.CheckInEnabled)
                    {
                        ReportProgress?.Invoke(this, $"[AZOPS] => Checking in changes.");
                        sourceControl.CheckInChanges();
                    }
                }

                telemetry.TrackCustomEventWithCustomMetrics("Deploy Finished", new MetricData("Project Name", project));
            }
            catch (Exception exception)
            {
                ReportProgress?.Invoke(this, $"[ERROR] => {exception.Message}\n");
                if (!(exception is ToolkitException))
                    telemetry.TrackExceptionWithCustomMetrics(exception);
            }
        }
    }
}
