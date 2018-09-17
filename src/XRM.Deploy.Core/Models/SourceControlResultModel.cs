using Microsoft.TeamFoundation.VersionControl.Client;

namespace XRM.Deploy.Core.Models
{
    internal class SourceControlResultModel
    {
        public SourceControlResultModel()
        {
            Continue = true;
        }

        public bool Continue { get; set; }

        public PendingChange[] Changes { get; set; }
    }
}
