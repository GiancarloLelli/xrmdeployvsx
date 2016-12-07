using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace XRM.Deploy.Vsix.Services
{
    internal class DteService
    {
        internal string GetPublishSettingsFilePathIfExist()
        {
            var vsEnvironment = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
            // Check for AvanadeToolkit.publishSettings inside Properties folder of selected project
            return string.Empty;
        }

        internal void LogMessage(string e, Guid pane)
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outWindow != null)
            {
                IVsOutputWindowPane customPane = null;
                outWindow.GetPane(ref pane, out customPane);

                if (customPane == null)
                {
                    outWindow.CreatePane(ref pane, "Avanade CRM Toolkit - Publish Output", 1, 1);
                    outWindow.GetPane(ref pane, out customPane);
                }

                if (customPane != null)
                {
                    customPane?.OutputString(string.Concat(e, Environment.NewLine));
                    customPane?.Activate();
                }
            }
        }

        internal string GetProprtiesFolderPath()
        {
            return string.Empty;
        }
    }
}
