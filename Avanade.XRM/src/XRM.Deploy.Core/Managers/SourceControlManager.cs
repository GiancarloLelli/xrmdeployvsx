using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Linq;
using XRM.Deploy.Core.Models;
using XRM.Telemetry;

namespace XRM.Deploy.Core.Managers
{
    internal class SourceControlManager
    {
        private readonly string m_workspace;
        private readonly string m_user;
        private readonly Action<string> m_progress;
        private readonly TelemetryWrapper m_telemetry;
        private readonly Uri m_tfs;
        private readonly VssCredentials m_creds;
        private SourceControlResultModel m_result;

        public SourceControlManager(string workspace, string user, Uri tfs, VssCredentials creds, Action<string> reportProgress, TelemetryWrapper telemetry)
        {
            m_workspace = workspace;
            m_user = user;
            m_progress = reportProgress;
            m_telemetry = telemetry;
            m_tfs = tfs;
            m_creds = creds;
            m_result = null;
        }

        public SourceControlResultModel InitializeWorkspace()
        {
            SourceControlResultModel result = new SourceControlResultModel();

            try
            {
                using (TfsTeamProjectCollection collection = new TfsTeamProjectCollection(m_tfs, m_creds))
                {
                    collection.EnsureAuthenticated();
                    m_progress?.Invoke("------------------- START -----------------------");
                    m_progress?.Invoke($"[TFS] => Detecting changes in the workspace.");
                    VersionControlServer versionControl = collection.GetService<VersionControlServer>();
                    Workspace workspace = versionControl.GetWorkspace(m_workspace, m_user);

                    result.Changes = workspace.GetPendingChanges();
                    m_result = result;

                    m_progress?.Invoke($"[TFS] => Found {result.Changes.Length} Check-Out items.");

                    var folderFilters = result.Changes.Select(c => c.LocalOrServerFolder).Distinct().ToArray();
                    if (folderFilters.Length > 0)
                    {
                        var conflicts = workspace.QueryConflicts(folderFilters, true);
                        if (conflicts.Length > 0)
                        {
                            m_progress?.Invoke($"[TFS] => Found {conflicts.Length} conflicts. Update your workspace to continue.");
                            result.Continue = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_progress?.Invoke($"[EXCEPTION] => {ex.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
                result.Continue = false;
            }

            return result;
        }

        public void CheckInChanges()
        {
            try
            {
                if (m_result != null)
                {
                    using (TfsTeamProjectCollection collection = new TfsTeamProjectCollection(m_tfs, m_creds))
                    {
                        VersionControlServer versionControl = collection.GetService(typeof(VersionControlServer)) as VersionControlServer;
                        Workspace workspace = versionControl.GetWorkspace(m_workspace, m_user);

                        if (m_result.Changes.Length > 0)
                        {
                            workspace.CheckIn(m_result.Changes, $"CI Checkin by {Environment.UserName}");
                            m_progress?.Invoke($"[TFS] => Check-In made on behalf of {Environment.UserName}.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_progress?.Invoke($"[EXCEPTION] => {ex.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
            }
        }
    }
}
