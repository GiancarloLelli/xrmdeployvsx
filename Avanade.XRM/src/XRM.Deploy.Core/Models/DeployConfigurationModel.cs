using System;

namespace Xrm.Deploy.Core.Models
{
	internal class DeployConfigurationModel
	{
		internal Uri TFS { get; set; }
		internal string Domain { get; set; }
		internal string User { get; set; }
		internal string Password { get; set; }
		internal string Solution { get; set; }
		internal string Prefix { get; set; }
		internal string CRMKey { get; set; }
		internal string Workspace { get; set; }
        internal string ConfigurationName { get; set; }

		internal bool UseConfigCredentials
		{
			get
			{
				return !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password);
			}
		}
	}
}
