using Avanade.XRM.Deployer.Model;
using Microsoft.TeamFoundation.Client;
using System.Net;

namespace Avanade.XRM.Deployer.Auth
{
	public class AuthHelper
	{
		public static TfsClientCredentials GetCredentials(DeployConfiguration con)
		{
			var credentials = new TfsClientCredentials(true);

			if (con.UseConfigCredentials)
			{
				credentials = new TfsClientCredentials(new WindowsCredential(new NetworkCredential(con.User, con.Password, con.Domain)));
			}

			return credentials;
		}
	}
}
