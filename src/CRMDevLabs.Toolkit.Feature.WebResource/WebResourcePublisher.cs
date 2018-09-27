using CRMDevLabs.Toolkit.Git;
using CRMDevLabs.Toolkit.Git.Models;
using CRMDevLabs.Toolkit.Models.Telemetry;
using CRMDevLabs.Toolkit.Models.Xrm;
using CRMDevLabs.Toolkit.Telemetry;
using CRMDevLabs.Toolkit.Xrm;
using System;
using System.Linq;

namespace CRMDevLabs.Toolkit.Feature.WebResource
{
    public class PublishOrchestrator
    {
        public event EventHandler<string> ReportProgress;

        public void Publish(DeployConfigurationModel deployConfiguration, TelemetryWrapper telemetry, string singleResourceName, string project, string projectPath, string basePath)
        {
            try
            {
                Action<string> reportAction = (m) => { ReportProgress?.Invoke(this, m); };
                var context = new XrmService(deployConfiguration.DynamicsSettings, deployConfiguration.Solution, telemetry, reportAction);
                var sourceControl = new VersioningService(projectPath, basePath, deployConfiguration.Branch, reportAction, telemetry);
                var sourceControlResult = sourceControl.QueryLocalRepository();

                // Must resolve conflicts or something went wrong with TFS interaction
                if (!sourceControlResult.Continue && sourceControlResult.Changes == null)
                    return;

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

        private void PublishImpl(XrmService context, VersioningService gitvc, DeployConfigurationModel deployConfiguration, TelemetryWrapper telemetry, RawChanges[] changes, string project)
        {
            try
            {
                ChangeManagerService container = new ChangeManagerService(changes, deployConfiguration.Prefix, context);

                if (container.WebResources.Count > 0)
                {
                    ReportProgress?.Invoke(this, $"[DYNAMICS] => Found {container.WebResources.Count} Web Resource.");
                    ReportProgress?.Invoke(this, $"[DYNAMICS] => '{deployConfiguration.Prefix}' used as base path.");
                    ReportProgress?.Invoke(this, $"[DYNAMICS] => Fetching '{deployConfiguration.Solution}' solution from CRM.");
                    container.EnsureContinue(deployConfiguration.Solution, deployConfiguration.Prefix);

                    ReportProgress?.Invoke(this, $"[DYNAMICS] => Publishing changes to the CRM.");
                    var faultedFlushResult = context.Flush(container.BuildRequestList(deployConfiguration.Solution));

                    if (!faultedFlushResult && deployConfiguration.CheckInEnabled)
                    {
                        ReportProgress?.Invoke(this, $"[AZOPS] => Commit & Push in progress.");
                        gitvc.CommitAndPush(deployConfiguration.Password);
                    }

                    ReportProgress?.Invoke(this, $"[AZOPS] => Publish completed.");
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
