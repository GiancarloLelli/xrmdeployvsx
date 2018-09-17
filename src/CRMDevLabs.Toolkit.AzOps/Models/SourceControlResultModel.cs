using Microsoft.TeamFoundation.VersionControl.Client;

namespace CRMDevLabs.Toolkit.AzOps.Models
{
    public class SourceControlResultModel
    {
        public SourceControlResultModel()
        {
            Continue = true;
        }

        public bool Continue { get; set; }

        public PendingChange[] Changes { get; set; }
    }
}
