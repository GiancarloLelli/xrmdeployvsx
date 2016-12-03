using Microsoft.TeamFoundation.VersionControl.Client;

namespace XRM.Deploy.Core.Models
{
	internal class SourceControlResultModel
	{
		internal SourceControlResultModel()
		{
			Continue = true;
		}

		internal bool Continue { get; set; }
		internal PendingChange[] Changes { get; set; }
	}
}
