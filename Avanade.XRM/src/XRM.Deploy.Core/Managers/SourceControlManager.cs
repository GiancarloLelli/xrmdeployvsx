using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
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
        private readonly TfsClientCredentials m_creds;
        private SourceControlResultModel m_result;

        public SourceControlManager(string workspace, string user, Uri tfs, TfsClientCredentials creds, Action<string> reportProgress, TelemetryWrapper telemetry)
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
                    m_progress?.Invoke("------------------- START -----------------------");
                    m_progress?.Invoke($"[TFS] => Enumerazione cambiamenti sul Workspace.");
                    VersionControlServer versionControl = collection.GetService(typeof(VersionControlServer)) as VersionControlServer;
                    Workspace workspace = versionControl.GetWorkspace(m_workspace, m_user);

                    result.Changes = workspace.GetPendingChanges();
                    m_result = result;

                    m_progress?.Invoke($"[TFS] => Trovate {result.Changes.Length} Check-Out items.");

                    var folderFilters = result.Changes.Select(c => c.LocalOrServerFolder).Distinct().ToArray();
                    if (folderFilters.Length > 0)
                    {
                        var conflicts = workspace.QueryConflicts(folderFilters, true);
                        if (conflicts.Length > 0)
                        {
                            m_progress?.Invoke($"[TFS] => Rilevati {conflicts.Length} conflitti. Aggiorna il workspace prima di continuare");
                            result.Continue = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_progress?.Invoke($"[EXCEPTION] => {ex.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
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
                            m_progress?.Invoke($"[TFS] => Check-In effettuato on behalf of {Environment.UserName}.");
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
