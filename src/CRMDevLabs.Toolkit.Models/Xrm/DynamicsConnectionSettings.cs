namespace CRMDevLabs.Toolkit.Models.Xrm
{
    public class DynamicsConnectionSettings
    {
        public string Discovery { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string OrganizationName { get; set; }

        public override string ToString()
        {
            // TODO: Change connection string Layout
            return base.ToString();
        }
    }
}
