using CRMDevLabs.Toolkit.Models.Telemetry;
using CRMDevLabs.Toolkit.Models.Xrm;
using Microsoft.VisualStudio.Services.Common;

namespace CRMDevLabs.Toolkit.AzOps.Providers
{
    public class CredentialsProvider
    {
        public static VssCredentials GetCredentials(DeployConfigurationModel con)
        {
            if (string.IsNullOrEmpty(con.SourceControlSettings.Pat))
                throw new ToolkitException("Invalid Azure DevOps authentication settings. Read the docs to properly confiure the toolkit.");

            var credentials = new VssBasicCredential(string.Empty, con.SourceControlSettings.Pat);

            return credentials;
        }
    }
}
