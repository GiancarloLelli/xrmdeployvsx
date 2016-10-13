using Avanade.XRM.Deployer.Model;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Linq;

namespace Avanade.XRM.Deployer.TFS
{
	public class SourceControlManager
	{
		readonly string m_workspace;
		readonly string m_user;

		public SourceControlManager(string workspace, string user)
		{
			m_workspace = workspace;
			m_user = user;
		}

		public SourceControlResult InitializeWorkspace(Uri tfs, TfsClientCredentials auth)
		{
			SourceControlResult result = new SourceControlResult();

			using (TfsTeamProjectCollection collection = new TfsTeamProjectCollection(tfs, auth))
			{
				Console.WriteLine($"[LOG] => Enumerazione cambiamenti sul Workspace.");
				VersionControlServer versionControl = collection.GetService(typeof(VersionControlServer)) as VersionControlServer;
				Workspace workspace = versionControl.GetWorkspace(m_workspace, m_user);

				result.Changes = workspace.GetPendingChanges();
				Console.WriteLine($"[LOG] => Trovate {result.Changes.Length} Check-Out items.");

				var folderFilters = result.Changes.Select(c => c.LocalOrServerFolder).Distinct().ToArray();
				if (folderFilters.Length > 0)
				{
					var conflicts = workspace.QueryConflicts(folderFilters, true);
					if (conflicts.Length > 0)
					{
						Console.WriteLine($"[LOG] => Rilevati {conflicts.Length} conflitti. Aggiorna il workspace prima di continuare");
						result.Continue = false;
					}
				}

				if (result.Changes.Length > 0)
				{
					workspace.CheckIn(result.Changes, $"CI Checkin by {Environment.UserName}");
					Console.WriteLine($"[LOG] => Check-In effettuato on behalf of {Environment.UserName}.");
				}

				return result;
			}
		}
	}
}
