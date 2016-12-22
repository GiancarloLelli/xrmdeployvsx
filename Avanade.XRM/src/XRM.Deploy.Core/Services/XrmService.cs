using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using XRM.Deploy.Core.Factories;
using XRM.Deploy.Core.Models;
using XRM.Telemetry;

namespace Avanade.XRM.Deployer.Service
{
    internal class XrmService
    {
        readonly Lazy<IOrganizationService> m_service;
        readonly Action<string> m_progress;

        internal XrmService(string connectionString, Action<string> report)
        {
            try
            {
                m_progress = report;
                m_service = new Lazy<IOrganizationService>(() =>
                {
                    var conn = new CrmServiceClient(connectionString);
                    var service = (IOrganizationService)conn.OrganizationWebProxyClient != null
                                ? (IOrganizationService)conn.OrganizationWebProxyClient
                                : (IOrganizationService)conn.OrganizationServiceProxy;

                    return service;
                });
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[EXCEPTION] => {exception.Message}");
                TelemetryWrapper.Instance.TrackExceptionWithCustomMetrics(exception);
            }
        }

        internal SolutionDetailModel GetSolutionByName(string name)
        {
            SolutionDetailModel solution = null;

            try
            {
                QueryExpression query = new QueryExpression("solution");
                query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, name);
                Entity result = m_service.Value.RetrieveMultiple(query).Entities.FirstOrDefault();
                if (result != null) solution = new SolutionDetailModel(result.Id);
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[EXCEPTION] => {exception.Message}");
                TelemetryWrapper.Instance.TrackExceptionWithCustomMetrics(exception);
            }

            return solution;
        }

        internal Tuple<OrganizationRequest, OrganizationRequest, OrganizationRequest> RequestFactory(string changeType, string resourceName, Entity webResource, string solution)
        {
            OrganizationRequest requestGeneral = null;
            OrganizationRequest requestPublish = null;
            OrganizationRequest requestAddToSolution = null;

            try
            {
                switch (changeType.ToLower())
                {
                    case "add":
                        var meta = GetMetadata("webresource");
                        requestGeneral = OrganizationRequestFactory.CreateFactory(webResource);
                        requestAddToSolution = OrganizationRequestFactory.AddSolutionFactory(false, 61, meta.MetadataId.Value, solution);
                        break;
                    case "edit":
                    case "delete":
                        var resourceId = GetResourceIdIfExist(resourceName);
                        if (resourceId != Guid.Empty)
                        {
                            if (changeType.Equals("delete"))
                            {
                                requestGeneral = OrganizationRequestFactory.DeleteFactory(new EntityReference("webresource", resourceId));
                            }
                            else
                            {
                                webResource.Id = resourceId;
                                requestGeneral = OrganizationRequestFactory.UpdateFactory(webResource);
                                requestPublish = OrganizationRequestFactory.PublishFactory(resourceId.ToString());
                            }
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[EXCEPTION] => {exception.Message}");
                TelemetryWrapper.Instance.TrackExceptionWithCustomMetrics(exception);
            }

            return new Tuple<OrganizationRequest, OrganizationRequest, OrganizationRequest>(requestGeneral, requestPublish, requestAddToSolution);
        }

        internal void Flush(IEnumerable<OrganizationRequest> reqs)
        {
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
                        Settings = new ExecuteMultipleSettings()
                        {
                            ReturnResponses = true,
                            ContinueOnError = true
                        }
                    };

                    req.Requests.AddRange(batch);
                    var res = m_service.Value.Execute(req) as ExecuteMultipleResponse;

                    if (res.IsFaulted)
                    {
                        foreach (var results in res.Responses.Where(r => r.Fault != null))
                        {
                            var index = results.RequestIndex;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[ERROR] => Index: {index} - Error: {results.Fault.Message}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[EXCEPTION] => {exception.Message}");
                TelemetryWrapper.Instance.TrackExceptionWithCustomMetrics(exception);
            }
        }

        private EntityMetadata GetMetadata(string logicalName)
        {
            var meta = default(EntityMetadata);

            try
            {
                RetrieveEntityRequest request = new RetrieveEntityRequest()
                {
                    LogicalName = logicalName
                };

                var resp = (RetrieveEntityResponse)m_service.Value.Execute(request);
                meta = resp.EntityMetadata;
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[EXCEPTION] => {exception.Message}");
                TelemetryWrapper.Instance.TrackExceptionWithCustomMetrics(exception);
            }

            return meta;
        }

        private Guid GetResourceIdIfExist(string name)
        {
            Guid id = Guid.Empty;

            try
            {
                QueryExpression query = new QueryExpression("webresource");
                query.TopCount = 1;

                var filter = query.Criteria.AddFilter(LogicalOperator.Or);
                filter.AddCondition("displayname", ConditionOperator.Equal, name);
                filter.AddCondition("name", ConditionOperator.Equal, name);

                Entity result = m_service.Value.RetrieveMultiple(query).Entities.FirstOrDefault();
                id = result != null ? result.Id : Guid.Empty;
            }
            catch (Exception exception)
            {
                m_progress?.Invoke($"[EXCEPTION] => {exception.Message}");
                TelemetryWrapper.Instance.TrackExceptionWithCustomMetrics(exception);
            }

            return id;
        }
    }
}
