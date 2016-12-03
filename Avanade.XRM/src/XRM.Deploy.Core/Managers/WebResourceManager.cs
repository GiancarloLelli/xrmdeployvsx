using Avanade.XRM.Deployer.Service;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XRM.Deploy.Core.Extensions;
using XRM.Deploy.Core.Models;

namespace XRM.Deploy.Core.Managers
{
	internal class WebResourceManager
	{
		readonly PendingChange[] m_changes;
		readonly XrmService m_context;

		internal int TotalChanges { get { return m_changes.Count(); } }
		internal int AddedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("add")); } }
		internal int DeletedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("delete")); } }
		internal int EditedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("edit")); } }

		internal List<WebResourceModel> WebResources { get; private set; }

		internal WebResourceManager(PendingChange[] changes, string prefix, XrmService wrapper)
		{
			m_changes = changes;
			m_context = wrapper;
			WebResources = GetWebResourceFromChanges(prefix);
		}

		private List<WebResourceModel> GetWebResourceFromChanges(string prefix)
		{
			List<WebResourceModel> resources = new List<WebResourceModel>();

			foreach (var change in m_changes)
			{
				var crmWebResource = new WebResourceModel(prefix)
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

		internal void EnsureContinue(string solution)
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

		internal List<OrganizationRequest> BuildRequestList(string solution)
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
