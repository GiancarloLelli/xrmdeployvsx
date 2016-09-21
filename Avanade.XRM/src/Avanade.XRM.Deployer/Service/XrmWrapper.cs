using Avanade.XRM.Deployer.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Avanade.XRM.Deployer.Service
{
    public class XrmWrapper
    {
        readonly IOrganizationService m_service;

        public XrmWrapper(string connectionString)
        {
            m_service = new OrganizationService(connectionString);
        }

        public SolutionData GetSolutionByName(string name)
        {
            SolutionData solution = null;

            QueryExpression query = new QueryExpression
            {
                EntityName = "solution"
            };

            query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, name);
            Entity result = m_service.RetrieveMultiple(query).Entities.FirstOrDefault();

            if (result != null)
            {
                solution = new SolutionData
                {
                    Id = result.Id
                };
            }

            return solution;
        }

        public Tuple<OrganizationRequest, OrganizationRequest, OrganizationRequest> RequestFactory(string changeType, string resourceName, Entity webResource, string solution)
        {
            OrganizationRequest requestGeneral = null;
            OrganizationRequest requestPublish = null;
            OrganizationRequest requestAddToSolution = null;

            switch (changeType)
            {
                case "add":
                    requestGeneral = new CreateRequest { Target = webResource };
                    var meta = GetMetadata("webresource");

                    requestAddToSolution = new AddSolutionComponentRequest()
                    {
                        AddRequiredComponents = false,
                        ComponentType = 61, // See: https://msdynamicscrmblog.wordpress.com/2013/08/06/rootcomponent-types-in-the-solution-xml-file-in-dynamics-crm-2011/
                        ComponentId = meta.MetadataId.Value,
                        SolutionUniqueName = solution
                    };
                    break;
                case "edit":
                case "delete":
                    var resourceId = GetResourceIdIfExist(resourceName);
                    if (resourceId != Guid.Empty)
                    {
                        if (changeType.Equals("delete"))
                        {
                            requestGeneral = new DeleteRequest { Target = new EntityReference("webresource", resourceId) };
                        }
                        else
                        {
                            webResource.Id = resourceId;
                            requestGeneral = new UpdateRequest { Target = webResource };
                            requestPublish = new PublishXmlRequest
                            {
                                ParameterXml = string.Concat("<importexportxml><webresources><webresource>{", resourceId.ToString(), "}</webresource></webresources></importexportxml>")
                            };
                        }
                    }
                    break;
            }

            return new Tuple<OrganizationRequest, OrganizationRequest, OrganizationRequest>(requestGeneral, requestPublish, requestAddToSolution);
        }

        public void Flush(IEnumerable<OrganizationRequest> reqs)
        {
            var chunk = 0;
            while (reqs.Skip(chunk).Any())
            {
                var batch = reqs.Skip(chunk).Take(500).ToList();
                chunk += 500;

                ExecuteMultipleRequest req = new ExecuteMultipleRequest()
                {
                    Settings = new ExecuteMultipleSettings()
                    {
                        ReturnResponses = true,
                        ContinueOnError = true
                    },
                    Requests = new OrganizationRequestCollection()
                };

                req.Requests.AddRange(batch);
                var res = (ExecuteMultipleResponse)m_service.Execute(req);

                if (res.IsFaulted)
                {
                    foreach (var results in res.Responses.Where(r => r.Fault != null))
                    {
                        var index = results.RequestIndex;

                        var createOperation = batch[index] as CreateRequest;
                        var updateOperation = batch[index] as UpdateRequest;
                        var stateOperation = batch[index] as DeleteRequest;
                        var publishOperation = batch[index] as PublishXmlRequest;

                        var entity = createOperation != null ? createOperation.Target
                                : (updateOperation != null ? updateOperation.Target : null);

                        var name = entity != null ? entity.GetAttributeValue<string>("displayname") : "Not available";

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ERROR] => Index: {index} - WebResource: {name} - Error: {results.Fault.Message}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
        }

        private EntityMetadata GetMetadata(string logicalName)
        {
            RetrieveEntityRequest request = new RetrieveEntityRequest()
            {
                LogicalName = logicalName
            };

            var resp = (RetrieveEntityResponse)m_service.Execute(request);
            return resp.EntityMetadata;
        }

        private Guid GetResourceIdIfExist(string name)
        {
            Guid id = Guid.Empty;

            QueryExpression query = new QueryExpression
            {
                EntityName = "webresource",
                TopCount = 1
            };

            var filter = query.Criteria.AddFilter(LogicalOperator.Or);
            filter.AddCondition("displayname", ConditionOperator.Equal, name);
            filter.AddCondition("name", ConditionOperator.Equal, name);

            Entity result = m_service.RetrieveMultiple(query).Entities.FirstOrDefault();

            if (result != null)
            {
                id = result.Id;
            }

            return id;
        }

    }
}
