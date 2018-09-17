using Microsoft.Xrm.Sdk;

namespace CRMDevLabs.Toolkit.Xrm.Models
{
    public class RequestFactoryResult
    {
        public OrganizationRequest General { get; set; }

        public OrganizationRequest Publish { get; set; }
    }
}
