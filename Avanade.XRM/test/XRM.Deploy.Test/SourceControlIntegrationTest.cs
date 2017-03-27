using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XRM.Deploy.Core.Managers;
using Microsoft.VisualStudio.Services.Common;
using System.Net;

namespace XRM.Deploy.Test
{
    [TestClass]
    public class SourceControlIntegrationTest
    {
        // AWARE: SourceControlManager, SourceControlResultModel need to be marked as public for this test to compile & run.
        // AWARE: Mark them internal once the Uni Test is finished.

        [TestMethod]
        public void CanExtensionConnectToTFSOnPremise()
        {
            var tfs = new Uri("https://<host>:<port>/tfs/<collection>/");
            var logAction = new Action<string>((x) => { Console.Write(x); });
            var credentials = new VssCredentials(new WindowsCredential(new NetworkCredential("user", "password", "domain")));
            //var sourceControl = new SourceControlManager(Environment.MachineName, Environment.UserName, tfs, credentials, logAction, null);
            //var result = sourceControl.InitializeWorkspace();
            //Assert.IsTrue(result.Continue);
        }

        [TestMethod]
        public void CanExtensionConnectToVSTS()
        {
            var tfs = new Uri("https://<project>.visualstudio.com/DefaultCollection");
            var logAction = new Action<string>((x) => { Console.Write(x); });
            var cred = new VssBasicCredential(string.Empty, "pat");
            //var sourceControl = new SourceControlManager(Environment.MachineName, "gcarlo.lelli@live.com", tfs, cred, logAction, null);
            //var result = sourceControl.InitializeWorkspace();
            //Assert.IsTrue(result.Continue);
        }
    }
}