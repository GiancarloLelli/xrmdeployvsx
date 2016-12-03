using Microsoft.TeamFoundation.Client;
using System.Net;
using Xrm.Deploy.Core.Models;

namespace XRM.Deploy.Core.Providers
{
	internal class CredentialsProvider
	{
		internal static TfsClientCredentials GetCredentials(DeployConfigurationModel con)
		{
			var credentials = new TfsClientCredentials(true);

			if (con.UseConfigCredentials)
			{
				var networkCredential = new NetworkCredential(con.User, con.Password, con.Domain);
				var windowsCredential = new WindowsCredential(networkCredential);
				credentials = new TfsClientCredentials(windowsCredential);
			}

			return credentials;
		}
	}
}
