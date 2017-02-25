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
        private readonly DTE2 m_environment;

        public string PropertiesDirectory { get; private set; }

        public string Version
        {
            get
            {
                return m_environment.Version;
            }
        }

        public DteService()
        {
            m_environment = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
        }

        public string GetPublishSettingsFilePathIfExist()
        {
            var selectedProjectBase = (m_environment.ActiveSolutionProjects as object[])?.FirstOrDefault();
            var selectedProject = selectedProjectBase as Project;
            var fullPath = selectedProject?.FileName;
            var projectName = $"{selectedProject?.Name}.csproj";
            PropertiesDirectory = fullPath?.Replace(projectName, "Properties");
            if (!Directory.Exists(PropertiesDirectory)) Directory.CreateDirectory(PropertiesDirectory);
            var publishFilePath = string.Concat(PropertiesDirectory, "\\AvanadeToolkit.publishSettings");
            return File.Exists(publishFilePath) ? publishFilePath : string.Empty;
        }

        public string GetSelectedProjectName()
        {
            var selectedProjectBase = (m_environment.ActiveSolutionProjects as object[])?.FirstOrDefault();
            var selectedProject = selectedProjectBase as Project;
            return selectedProject?.FileName;
        }

        public string GetSelectedObjectName()
        {
            var fileName = string.Empty;
            var projectSelectedItems = m_environment.ToolWindows.SolutionExplorer.SelectedItems as object[];

            if (projectSelectedItems != null)
            {
                var firstObjectitem = projectSelectedItems.FirstOrDefault() as UIHierarchyItem;
                if (firstObjectitem != null)
                {
                    var hierarchyItem = (firstObjectitem as UIHierarchyItem)?.Object as ProjectItem;
                    var fullPath = hierarchyItem.Properties.Item("FullPath").Value.ToString();
                    fileName = firstObjectitem.IsSelected ? fullPath : string.Empty;
                }
            }

            return fileName;
        }

        public string GetSelectedProjectNameForAnalytics()
        {
            var selectedProjectBase = (m_environment.ActiveSolutionProjects as object[])?.FirstOrDefault();
            var selectedProject = selectedProjectBase as Project;
            return selectedProject?.Name;
        }

        public void LogMessage(string e, Guid pane)
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
