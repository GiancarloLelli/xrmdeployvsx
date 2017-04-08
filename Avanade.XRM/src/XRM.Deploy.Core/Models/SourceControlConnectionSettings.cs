namespace XRM.Deploy.Core.Models
{
    public class SourceControlConnectionSettings
    {
        public SourceControlConnectionSettings()
        {
            IsOnPrem = true;
            IsOnline = false;
        }

        public string TFSCollectionUrl { get; set; }

        public bool IsOnPrem { get; set; }

        public bool IsOnline { get; set; }

        public string Domain { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Pat { get; set; }
    }
}
