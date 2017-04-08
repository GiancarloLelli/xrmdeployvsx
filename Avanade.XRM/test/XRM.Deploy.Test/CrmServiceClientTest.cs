using Microsoft.VisualStudio.TestTools.UnitTesting;
using XRM.Sdk.Dynamics;

namespace XRM.Deploy.Test
{
    [TestClass]
    public class CrmServiceClientTest
    {
        [TestMethod]
        public void IOrganizationServiceFactoryTest()
        {
            var client = new CrmServiceClient("http://vas032:7777/XRMServices/2011/Discovery.svc", "ChiedimiTest2", "CRM_SETUP_SVIL", "Cattolica1", "APPCAT");
            var svc = client.OrganizationServiceFactory();
        }
    }
}
