namespace CRMDevLabs.Toolkit.Models.Xrm
{
    public class DeployConfigurationModel
    {
        public DeployConfigurationModel()
        {
            DynamicsSettings = new DynamicsConnectionSettings();
        }

        public DynamicsConnectionSettings DynamicsSettings { get; set; }

        public string Solution { get; set; }

        public string Prefix { get; set; }

        public bool CheckInEnabled { get; set; }

        public string Branch { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
