using System;

namespace Avanade.XRM.Deployer.Model
{
	public class DeployConfiguration
	{
		public Uri TFS { get; set; }
		public string Domain { get; set; }
		public string User { get; set; }
		public string Password { get; set; }
		public string Solution { get; set; }
		public string Prefix { get; set; }
		public string CRMKey { get; set; }
		public string Workspace { get; set; }

		public bool UseWindowsCredentials
		{
			get
			{
				return !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password);
			}
		}
	}
}
