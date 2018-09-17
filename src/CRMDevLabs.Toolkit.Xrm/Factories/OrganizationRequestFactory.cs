using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;

namespace CRMDevLabs.Toolkit.Xrm.Factories
{
    public class OrganizationRequestFactory
    {
        public static CreateRequest CreateFactory(Entity target) => new CreateRequest { Target = target };

        public static AddSolutionComponentRequest AddSolutionFactory(bool addRequired, int componentType, Guid componentId, string solution)
        {
            return new AddSolutionComponentRequest
            {
                AddRequiredComponents = addRequired,
                ComponentType = componentType,
                ComponentId = componentId,
                SolutionUniqueName = solution
            };
        }

        public static DeleteRequest DeleteFactory(EntityReference target) => new DeleteRequest { Target = target };

        public static UpdateRequest UpdateFactory(Entity target) => new UpdateRequest { Target = target };

        public static PublishXmlRequest PublishFactory(string id)
        {
            return new PublishXmlRequest
            {
                ParameterXml = string.Concat("<importexportxml><webresources><webresource>{", id, "}</webresource></webresources></importexportxml>")
            };
        }
    }
}
