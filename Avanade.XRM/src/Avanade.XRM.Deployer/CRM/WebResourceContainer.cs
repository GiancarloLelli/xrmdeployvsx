using Avanade.XRM.Deployer.Extensions;
using Avanade.XRM.Deployer.Model;
using Avanade.XRM.Deployer.Service;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Avanade.XRM.Deployer.CRM
{
	public class WebResourceContainer
	{
		readonly PendingChange[] m_changes;
		readonly XrmWrapper m_context;

		public int TotalChanges { get { return m_changes.Count(); } }
		public int AddedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("add")); } }
		public int DeletedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("delete")); } }
		public int EditedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("edit")); } }

		public List<CrmWebResource> WebResources { get; private set; }

		public WebResourceContainer(PendingChange[] changes, string prefix, XrmWrapper wrapper)
		{
			m_changes = changes;
			m_context = wrapper;
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

			if (!Directory.Exists("Build Logs")) Directory.CreateDirectory("Build Logs");
			foreach (var change in resources)
			{
				var timestamp = $"{DateTime.Now.Day}{DateTime.Now.Month}{DateTime.Now.Year}{DateTime.Now.Second}";
				File.WriteAllText($"Build Logs\\Build-Content-{timestamp}.txt", $"{change.ChangeType}\t{change.File}");
			}

			return resources;
		}

		public void EnsureContinue(string solution)
		{
			if (AddedItems > 0)
			{
				var solutionData = m_context.GetSolutionByName(solution);
				if (solutionData.Id == Guid.Empty)
				{
					throw new Exception($"No solution found in CRM with unique name: '{solution}'");
				}
			};
		}

		public List<OrganizationRequest> BuildRequestList(string solution)
		{
			List<OrganizationRequest> requests = new List<OrganizationRequest>();

			foreach (var resource in WebResources)
			{
				if (string.IsNullOrEmpty(resource.FullName)) continue;
				var factory = m_context.RequestFactory(resource.ChangeType, resource.FullName, resource.ToEntity(), solution);

				OrganizationRequest request = factory.Item1;
				OrganizationRequest publishRequest = factory.Item2;

				request.SetIfNotNull("SolutionUniqueName", solution);
				requests.AddIfNotNull(request);
				requests.AddIfNotNull(publishRequest);
			}

			return requests;
		}
	}
}
