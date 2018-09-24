using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.IO;
using System.Linq;

namespace CRMDevLabs.Toolkit.Services
{
    public class DteService
    {
        private readonly DTE2 m_environment;
        private readonly IVsSettingsManager m_provider;

        public string PropertiesDirectory { get; private set; }

        public string Version
        {
            get
            {
                return m_environment.Version;
            }
        }

        public SettingsStore ReadOnlySettings
        {
            get
            {
                SettingsStore store = null;

                if (store == null)
                {
                    var settingsManager = new ShellSettingsManager(m_provider);
                    store = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
                }

                return store;
            }
        }

        public WritableSettingsStore WritableSettings
        {
            get
            {
                WritableSettingsStore store = null;

                if (store == null)
                {
                    var settingsManager = new ShellSettingsManager(m_provider);
                    store = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
                }

                return store;
            }
        }

        public DteService(IVsSettingsManager provider)
        {
            m_environment = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
            m_provider = provider;
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

        public string GetSolutionRootPath()
        {
            var fullPath = m_environment.Solution?.FullName;

            var path = string.Empty;
            if (!string.IsNullOrEmpty(fullPath))
            {
                var info = new FileInfo(fullPath);
                path = info.Directory.FullName;
            }

            return path;
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
                    outWindow.CreatePane(ref pane, "Avanade Dynamics 365 Toolkit - Publish Output", 1, 1);
                    outWindow.GetPane(ref pane, out customPane);
                }

                if (customPane != null)
                {
                    customPane.OutputString(string.Concat(e, Environment.NewLine));
                    customPane.Activate();
                }
            }
        }

        public string GetProjectBasePath()
        {
            var selectedProjectBase = (m_environment.ActiveSolutionProjects as object[])?.FirstOrDefault();
            var selectedProject = selectedProjectBase as Project;
            var fullPath = selectedProject?.FileName;
            return new FileInfo(fullPath).DirectoryName;
        }
    }
}
