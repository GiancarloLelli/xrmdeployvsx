using XRM.Sdk.Dynamics.Models;

namespace XRM.Deploy.Core.Models
{
    public class DeployConfigurationModel
    {
        public DeployConfigurationModel()
        {
            SourceControlSettings = new SourceControlConnectionSettings();
            CrmSettings = new CrmConnectionSettings();
        }

        public CrmConnectionSettings CrmSettings { get; set; }

        public SourceControlConnectionSettings SourceControlSettings { get; set; }

        public string Solution { get; set; }

        public string Prefix { get; set; }

        public string Workspace { get; set; }

        public bool CheckInEnabled { get; set; }

        public bool UseConfigCredentials
        {
            get
            {
                return (!string.IsNullOrEmpty(SourceControlSettings.User) && !string.IsNullOrEmpty(SourceControlSettings.Password)) ||
                        !string.IsNullOrEmpty(SourceControlSettings.Pat);
            }
        }
    }
}
