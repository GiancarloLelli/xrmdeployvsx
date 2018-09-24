using CRMDevLabs.Toolkit.Models.AzOps;

namespace CRMDevLabs.Toolkit.Models.Xrm
{
    public class DeployConfigurationModel
    {
        public DeployConfigurationModel()
        {
            SourceControlSettings = new SourceControlConnectionSettings();
            DynamicsSettings = new DynamicsConnectionSettings();
        }

        public DynamicsConnectionSettings DynamicsSettings { get; set; }

        public SourceControlConnectionSettings SourceControlSettings { get; set; }

        public string Solution { get; set; }

        public string Prefix { get; set; }

        public string Workspace { get; set; }
    }
}
