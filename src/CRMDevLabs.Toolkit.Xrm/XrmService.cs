using CRMDevLabs.Toolkit.Models.Xrm;
using CRMDevLabs.Toolkit.Telemetry;
using CRMDevLabs.Toolkit.Xrm.Factories;
using CRMDevLabs.Toolkit.Xrm.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRMDevLabs.Toolkit.Xrm
{
    public class XrmService
    {
        private readonly Lazy<IOrganizationService> m_service;
        private readonly Action<string> m_progress;
        private readonly TelemetryWrapper m_telemetry;
        private readonly string m_solution;

        public XrmService(DynamicsConnectionSettings connectionSettings, string solution, TelemetryWrapper telemetry, Action<string> report)
        {
            try
            {
                m_progress = report;
                m_telemetry = telemetry;
                m_solution = solution;

                m_service = new Lazy<IOrganizationService>(() =>
                {
                    var crmServiceClient = new CrmServiceClient(connectionSettings.ToString());
                    IOrganizationService service = crmServiceClient as IOrganizationService;

                    if (service == null)
                        throw new NullReferenceException("Unable to instantiate IOrganizationService");

                    return service;
                });
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[ERROR] => {exception.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(exception);
            }
        }

        public SolutionDetailModel GetSolutionByName(string name)
        {
            SolutionDetailModel solution = null;

            try
            {
                var query = new QueryExpression("solution");
                query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, name);
                var result = m_service.Value.RetrieveMultiple(query).Entities.FirstOrDefault();
                if (result != null) solution = new SolutionDetailModel(result.Id);
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[ERROR] => {exception.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(exception);
            }

            return solution;
        }

        public RequestFactoryResult RequestFactory(string changeType, string resourceName, Entity webResource)
        {
            var requestFactoryResult = new RequestFactoryResult();

            try
            {
                switch (changeType)
                {
                    case "NewInWorkdir":
                        requestFactoryResult.General = OrganizationRequestFactory.CreateFactory(webResource);
                        break;
                    case "ModifiedInWorkdir":
                    case "DeletedFromWorkdir":
                        var resourceId = GetResourceIdIfExist(resourceName);
                        if (resourceId != Guid.Empty)
                        {
                            if (changeType.Equals("DeletedFromWorkdir"))
                            {
                                requestFactoryResult.General = OrganizationRequestFactory.DeleteFactory(new EntityReference("webresource", resourceId));
                            }
                            else
                            {
                                webResource.Id = resourceId;
                                requestFactoryResult.General = OrganizationRequestFactory.UpdateFactory(webResource);
                                requestFactoryResult.Publish = OrganizationRequestFactory.PublishFactory(resourceId.ToString());
                            }
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[ERROR] => {exception.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(exception);
            }

            return requestFactoryResult;
        }

        public bool Flush(List<OrganizationRequest> reqs)
        {
            bool isFaulted = false;
            var addToSolutionRequests = new List<OrganizationRequest>();

            try
            {
                var chunk = 0;
                while (reqs.Skip(chunk).Any())
                {
                    var batch = reqs.Skip(chunk).Take(500).ToList();
                    chunk += 500;

                    ExecuteMultipleRequest req = new ExecuteMultipleRequest()
                    {
                        Requests = new OrganizationRequestCollection(),
                        Settings = new ExecuteMultipleSettings { ReturnResponses = true, ContinueOnError = false }
                    };

                    req.Requests.AddRange(batch);
                    var res = m_service.Value.Execute(req) as ExecuteMultipleResponse;
                    isFaulted = res.IsFaulted;

                    if (res.IsFaulted)
                    {
                        foreach (var results in res.Responses.Where(r => r.Fault != null))
                        {
                            var index = results.RequestIndex;
                            m_progress?.Invoke($"[ERROR] => Index: {index} - Error: {results.Fault.Message}");
                        }
                    }
                    else
                    {
                        var createResponse = res.Responses.Where(x => (x.Response as CreateResponse) != null);
                        foreach (var creationResponseItem in createResponse)
                        {
                            var creationRsponse = creationResponseItem.Response as CreateResponse;
                            addToSolutionRequests.Add(OrganizationRequestFactory.AddSolutionFactory(false, 61, creationRsponse.id, m_solution));
                        }
                    }
                }

                if (addToSolutionRequests.Count > 0)
                    isFaulted = isFaulted || Flush(addToSolutionRequests);
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[ERROR] => {exception.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(exception);
            }

            return isFaulted;
        }

        private EntityMetadata GetMetadata(string logicalName)
        {
            var meta = default(EntityMetadata);

            try
            {
                var request = new RetrieveEntityRequest { LogicalName = logicalName };
                var resp = m_service.Value.Execute(request) as RetrieveEntityResponse;
                meta = resp.EntityMetadata;
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[ERROR] => {exception.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(exception);
            }

            return meta;
        }

        private Guid GetResourceIdIfExist(string name)
        {
            Guid id = Guid.Empty;

            try
            {
                var query = new QueryExpression("webresource");
                query.TopCount = 1;

                var filter = query.Criteria.AddFilter(LogicalOperator.Or);
                filter.AddCondition("displayname", ConditionOperator.Equal, name);
                filter.AddCondition("name", ConditionOperator.Equal, name);

                Entity result = m_service.Value.RetrieveMultiple(query).Entities.FirstOrDefault();
                id = result != null ? result.Id : Guid.Empty;
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[ERROR] => {exception.Message}");
                m_telemetry.TrackExceptionWithCustomMetrics(exception);
            }

            return id;
        }
    }
}
