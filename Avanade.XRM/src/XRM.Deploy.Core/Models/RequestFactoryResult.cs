using Microsoft.Xrm.Sdk;

namespace XRM.Deploy.Core.Models
{
    internal class RequestFactoryResult
    {
        public OrganizationRequest General { get; set; }

        public OrganizationRequest AddToSolution { get; set; }

        public OrganizationRequest Publish { get; set; }
    }
}
