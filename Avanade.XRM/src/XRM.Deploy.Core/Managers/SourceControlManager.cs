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
        readonly string m_workspace;
        readonly string m_user;
        readonly Action<string> m_progress;
        readonly TelemetryWrapper m_telemetry;

        internal SourceControlManager(string workspace, string user, Action<string> reportProgress, TelemetryWrapper telemetry)
        {
            m_workspace = workspace;
            m_user = user;
            m_progress = reportProgress;
            m_telemetry = telemetry;
        }

        internal SourceControlResultModel InitializeWorkspace(Uri tfs, TfsClientCredentials auth, bool checkin)
        {
            SourceControlResultModel result = new SourceControlResultModel();
            try
            {
                using (TfsTeamProjectCollection collection = new TfsTeamProjectCollection(tfs, auth))
                {
                    m_progress?.Invoke("------------------- START -----------------------");
                    m_progress?.Invoke($"[TFS] => Enumerazione cambiamenti sul Workspace.");
                    VersionControlServer versionControl = collection.GetService(typeof(VersionControlServer)) as VersionControlServer;
                    Workspace workspace = versionControl.GetWorkspace(m_workspace, m_user);

                    result.Changes = workspace.GetPendingChanges();
                    m_progress?.Invoke($"[TFS] => Trovate {result.Changes.Length} Check-Out items.");

                    var folderFilters = result.Changes.Select(c => c.LocalOrServerFolder).Distinct().ToArray();
                    if (folderFilters.Length > 0 && checkin)
                    {
                        var conflicts = workspace.QueryConflicts(folderFilters, true);
                        if (conflicts.Length > 0)
                        {
                            m_progress?.Invoke($"[TFS] => Rilevati {conflicts.Length} conflitti. Aggiorna il workspace prima di continuare");
                            result.Continue = false;
                        }
                    }

                    if (result.Changes.Length > 0 && checkin)
                    {
                        workspace.CheckIn(result.Changes, $"CI Checkin by {Environment.UserName}");
                        m_progress?.Invoke($"[TFS] => Check-In effettuato on behalf of {Environment.UserName}.");
                    }
                }
            }
            catch (Exception ex)
            {
                m_progress?.Invoke($"[EXCEPTION] => {ex.Message}");
                m_telemetry.Instance.TrackExceptionWithCustomMetrics(ex);
            }

            return result;
        }
    }
}
