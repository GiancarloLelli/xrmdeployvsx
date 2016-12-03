using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XRM.Deploy.Core.Factories
{
	internal class OrganizationRequestFactory
	{
		internal static CreateRequest CreateFactory(Entity target) => new CreateRequest { Target = target };

		internal static AddSolutionComponentRequest AddSolutionFactory(bool addRequired, int componentType, Guid componentId, string solution)
		{
			return new AddSolutionComponentRequest
			{
				AddRequiredComponents = addRequired,
				ComponentType = componentType,
				ComponentId = componentId,
				SolutionUniqueName = solution
			};
		}

		internal static DeleteRequest DeleteFactory(EntityReference target) => new DeleteRequest { Target = target };

		internal static UpdateRequest UpdateFactory(Entity target) => new UpdateRequest { Target = target };

		internal static PublishXmlRequest PublishFactory(string id)
		{
			return new PublishXmlRequest
			{
				ParameterXml = string.Concat("<importexportxml><webresources><webresource>{", id, "}</webresource></webresources></importexportxml>")
			};
		}
	}
}
