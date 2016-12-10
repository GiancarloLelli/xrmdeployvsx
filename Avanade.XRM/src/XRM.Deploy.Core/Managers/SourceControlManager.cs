using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Linq;
using XRM.Deploy.Core.Models;

namespace XRM.Deploy.Core.Managers
{
    internal class SourceControlManager
    {
        readonly string m_workspace;
        readonly string m_user;
        readonly Action<string> m_progress;

        internal SourceControlManager(string workspace, string user, Action<string> reportProgress)
        {
            m_workspace = workspace;
            m_user = user;
            m_progress = reportProgress;
        }

        internal SourceControlResultModel InitializeWorkspace(Uri tfs, TfsClientCredentials auth, bool checkin)
        {
            SourceControlResultModel result = new SourceControlResultModel();

            using (TfsTeamProjectCollection collection = new TfsTeamProjectCollection(tfs, auth))
            {
                m_progress?.Invoke($"[LOG] => Enumerazione cambiamenti sul Workspace.");
                VersionControlServer versionControl = collection.GetService(typeof(VersionControlServer)) as VersionControlServer;
                Workspace workspace = versionControl.GetWorkspace(m_workspace, m_user);

                result.Changes = workspace.GetPendingChanges();
                m_progress?.Invoke($"[LOG] => Trovate {result.Changes.Length} Check-Out items.");

                var folderFilters = result.Changes.Select(c => c.LocalOrServerFolder).Distinct().ToArray();
                if (folderFilters.Length > 0 && checkin)
                {
                    var conflicts = workspace.QueryConflicts(folderFilters, true);
                    if (conflicts.Length > 0)
                    {
                        m_progress?.Invoke($"[LOG] => Rilevati {conflicts.Length} conflitti. Aggiorna il workspace prima di continuare");
                        result.Continue = false;
                    }
                }

                if (result.Changes.Length > 0 && checkin)
                {
                    workspace.CheckIn(result.Changes, $"CI Checkin by {Environment.UserName}");
                    m_progress?.Invoke($"[LOG] => Check-In effettuato on behalf of {Environment.UserName}.");
                }

                return result;
            }
        }
    }
}
