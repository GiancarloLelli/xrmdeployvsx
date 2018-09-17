using Microsoft.VisualStudio.Services.Common;
using System.Net;
using XRM.Deploy.Core.Fallback;
using XRM.Deploy.Core.Models;

namespace XRM.Deploy.Core.Providers
{
    internal class CredentialsProvider
    {
        public static VssCredentials GetCredentials(DeployConfigurationModel con)
        {
            // Defaults to NTLM authentication
            var credentials = new VssCredentials(true);

            if (con.UseConfigCredentials)
            {
                if (con.SourceControlSettings.IsOnline)
                {
                    // VSTS - Personal Access Token
                    credentials = new VssBasicCredential(string.Empty, con.SourceControlSettings.Pat);
                }
                else if (con.SourceControlSettings.IsOnPrem)
                {
                    // TFS On Premise - domain\user & password authentication
                    var networkCredential = new NetworkCredential(con.SourceControlSettings.User, con.SourceControlSettings.Password, con.SourceControlSettings.Domain);
                    var windowsCredential = new WindowsCredential(networkCredential);
                    credentials = new VssCredentials(windowsCredential);
                }
                else
                {
                    throw new ToolkitException("Invalid TFS/VSTS authentication settings. Read the docs to properly confiure the toolkit.");
                }
            }

            return credentials;
        }
    }
}
