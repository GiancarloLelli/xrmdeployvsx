namespace CRMDevLabs.Toolkit.Git.Models
{
    public class SourceControlResultModel
    {
        public SourceControlResultModel()
        {
            Continue = true;
        }

        public bool Continue { get; set; }

        public RawChanges[] Changes { get; set; }
    }

    public class RawChanges
    {
        public string FileName { get; set; }
        public string ChangeTypeName { get; set; }
        public string LocalItem { get; set; }
    }
}
