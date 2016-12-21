using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;

namespace XRM.Deploy.Vsix.Services
{
    public class DteService
    {
        readonly DTE2 m_environment;

        internal string PropertiesDirectory { get; private set; }

        internal string Version
        {
            get
            {
                return m_environment.Version;
            }
        }

        internal DteService()
        {
            m_environment = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
        }

        internal string GetPublishSettingsFilePathIfExist()
        {
            var selectedProjectBase = (m_environment.ActiveSolutionProjects as object[])?.FirstOrDefault();
            var selectedProject = selectedProjectBase as Project;
            var fullPath = selectedProject?.FileName;
            var projectName = $"{selectedProject?.Name}.csproj";
            PropertiesDirectory = fullPath?.Replace(projectName, "Properties");
            var publishFilePath = string.Concat(PropertiesDirectory, "\\AvanadeToolkit.publishSettings");
            return File.Exists(publishFilePath) ? publishFilePath : string.Empty;
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
                    customPane.OutputString(string.Concat(e, Environment.NewLine));
                    customPane.Activate();
                }
            }
        }
    }
}
