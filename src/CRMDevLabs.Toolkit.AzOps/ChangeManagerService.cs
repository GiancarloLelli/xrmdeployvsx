using CRMDevLabs.Toolkit.Common.Extensions;
using CRMDevLabs.Toolkit.Models.Telemetry;
using CRMDevLabs.Toolkit.Xrm;
using CRMDevLabs.Toolkit.Xrm.Extensions;
using CRMDevLabs.Toolkit.Xrm.Models;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRMDevLabs.Toolkit.AzOps
{
    public class ChangeManagerService
    {
        private readonly PendingChange[] m_changes;
        private readonly XrmService m_context;

        public int TotalChanges { get { return m_changes.Count(); } }

        public int AddedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("add")); } }

        public int DeletedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("delete")); } }

        public int EditedItems { get { return m_changes.Count(p => p.ChangeTypeName.ToLower().Equals("edit")); } }

        public List<WebResourceModel> WebResources { get; private set; }

        public ChangeManagerService(PendingChange[] changes, string prefix, XrmService wrapper)
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

        public void EnsureContinue(string solution, string root)
        {
            if (AddedItems > 0)
            {
                var solutionData = m_context.GetSolutionByName(solution);
                if (solutionData.Id == Guid.Empty)
                {
                    throw new ToolkitException($"No solution found in CRM with unique name: '{solution}'");
                }
            };
        }

        public List<OrganizationRequest> BuildRequestList(string solution)
        {
            List<OrganizationRequest> requests = new List<OrganizationRequest>();

            foreach (var resource in WebResources)
            {
                if (string.IsNullOrEmpty(resource.FullName)) continue;
                var factory = m_context.RequestFactory(resource.ChangeType, resource.FullName, resource.ToEntity());

                factory.General.SetIfNotNull("SolutionUniqueName", solution);
                requests.AddIfNotNull(factory.General);
                requests.AddIfNotNull(factory.Publish);
            }

            return requests;
        }
    }
}
