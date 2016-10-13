using Microsoft.TeamFoundation.VersionControl.Client;

namespace Avanade.XRM.Deployer.Model
{
	public class SourceControlResult
	{
		public SourceControlResult()
		{
			Continue = true;
		}

		public bool Continue { get; set; }
		public PendingChange[] Changes { get; set; }
	}
}
