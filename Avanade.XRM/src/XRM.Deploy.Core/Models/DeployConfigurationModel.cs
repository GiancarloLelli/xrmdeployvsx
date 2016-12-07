using System;

namespace Xrm.Deploy.Core.Models
{
    public class DeployConfigurationModel
    {
        public Uri TFSCollectionUrl { get; set; }
        public string Domain { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Solution { get; set; }
        public string Prefix { get; set; }
        public string CRMConnectionString { get; set; }
        public string Workspace { get; set; }
        public string ConfigurationName { get; set; }
        public bool CheckInEnabled { get; set; }

        public bool UseConfigCredentials
        {
            get
            {
                return !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password);
            }
        }

    }
}
