using Avanade.XRM.Deployer.Model;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections.Generic;
using System.Linq;

namespace Avanade.XRM.Deployer.CRM
{
	public class WebResourceContainer
	{
		readonly PendingChange[] m_changes;

		public int TotalChanges { get { return m_changes.Count(); } }
		public int AddedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("add")); } }
		public int DeletedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("delete")); } }
		public int EditedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("edit")); } }

		public List<CrmWebResource> WebResources { get; private set; }

		public WebResourceContainer(PendingChange[] changes, string prefix)
		{
			m_changes = changes;
			WebResources = GetWebResourceFromChanges(prefix);
		}

		private List<CrmWebResource> GetWebResourceFromChanges(string prefix)
		{
			List<CrmWebResource> resources = new List<CrmWebResource>();

			foreach (var change in m_changes)
			{
				var crmWebResource = new CrmWebResource(prefix)
				{
					ChangeType = change.ChangeTypeName,
					File = change.LocalItem,
					DisplayName = change.FileName
				};

				if (crmWebResource.FileType == 99) continue;
				resources.Add(crmWebResource);
			}

			return resources;
		}
	}
}
