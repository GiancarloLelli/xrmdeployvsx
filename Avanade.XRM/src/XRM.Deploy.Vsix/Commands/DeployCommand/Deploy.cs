using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.ComponentModel.Design;
using Xrm.Deploy.Vsix.Helpers;
using XRM.Deploy.Core;
using XRM.Deploy.Vsix.Models;
using XRM.Deploy.Vsix.Services;
using XRM.Deploy.Vsix.ViewModels;
using XRM.Deploy.Vsix.Views;
using XRM.Telemetry;
using XRM.Telemetry.Helpers;
using XRM.Telemetry.Models;
using Async = System.Threading.Tasks;

namespace XRM.Deploy.Vsix.Commands.DeployCommand
{
    internal sealed class Deploy
    {
        private Guid m_pane;
        readonly Package m_package;
        readonly DteService m_service;
        readonly TelemetryWrapper m_telemetry;
        public const int DeployCommandId = 0x1023;
        public const int InitCommandId = 0x1022;
        public static readonly Guid CommandSet = new Guid("222af809-1598-498f-a9d7-6b130d420527");

        public static Deploy Instance { get; private set; }

        private IServiceProvider ServiceProvider { get { return m_package; } }

        public static void Initialize(Package package) => Instance = new Deploy(package);

        private Deploy(Package package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));

            m_package = package;
            m_pane = new Guid("A8E3D03E-28C9-4900-BD48-CEEDEC35E7E6");
            m_service = new DteService();
            m_telemetry = new TelemetryWrapper(m_service.Version, VersionHelper.GetVersionFromManifest());

            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var deployMenuCommandID = new CommandID(CommandSet, DeployCommandId);
                var deployMenuItem = new OleMenuCommand(DeployMenuItemCallback, HandleMenuItemChange, HandleMenuItemVisibility, deployMenuCommandID);
                commandService.AddCommand(deployMenuItem);

                var initializeMenuCommandId = new CommandID(CommandSet, InitCommandId);
                var initMenuItem = new OleMenuCommand(InitMenuItemCallback, HandleMenuItemChange, HandleMenuItemVisibility, initializeMenuCommandId);
                commandService.AddCommand(initMenuItem);
            }
        }

        private void HandleMenuItemVisibility(object sender, EventArgs e)
        {
            var menu = sender as OleMenuCommand;
            var isDeploy = menu.CommandID.ID == DeployCommandId;

            var shellSettingsManager = new ShellSettingsManager(ServiceProvider);
            var settingsStore = shellSettingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
            var isDeployEnabled = settingsStore.GetBoolean("CRMToolkit", m_service.GetSelectedProjectName(), false);

            var menuService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            var initCommand = menuService.FindCommand(new CommandID(CommandSet, InitCommandId));

            if (initCommand != null) initCommand.Enabled = !isDeployEnabled;
            if (isDeploy) menu.Enabled = isDeployEnabled;
        }

        private void DeployMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                var projectName = m_service.GetSelectedProjectNameForAnalytics();
                m_telemetry.TrackCustomEventWithCustomMetrics("Deploy Started", new MetricData("Project Name", projectName));

                var publishSettigsPath = m_service.GetPublishSettingsFilePathIfExist();
                if (string.IsNullOrEmpty(publishSettigsPath))
                {
                    var dialog = new NewPublishSettingsPage(m_service, m_telemetry);
                    dialog.ShowDialog();
                    publishSettigsPath = (dialog.DataContext as NewPublishSettingsPageViewModel)?.FilePath;
                }

                // No valid configuration found or provided
                if (string.IsNullOrEmpty(publishSettigsPath)) return;

                var deployConfiguration = XmlObjectsHelper.Deserialize<DeployConfigurationModelFacade>(publishSettigsPath);
                var orchestrator = new PublishOrchestrator();
                orchestrator.ReportProgress += LogProgress;
                Async.Task.Factory.StartNew(() =>
                {
                    orchestrator.Publish(deployConfiguration.InnerObject, m_telemetry);
                    orchestrator.ReportProgress -= LogProgress;
                });

                m_telemetry.TrackCustomEventWithCustomMetrics("Deploy Finished", new MetricData("Project Name", projectName));
            }
            catch (Exception ex)
            {
                m_service.LogMessage($"[EXCEPTION] => {ex.Message}", m_pane);
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
            }
        }

        private void InitMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                m_telemetry.TrackCustomEventWithCustomMetrics("Project Initialization", new MetricData("Project Name", m_service.GetSelectedProjectNameForAnalytics()));

                var shellSettingsManager = new ShellSettingsManager(ServiceProvider);
                var settingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
                if (!settingsStore.CollectionExists("CRMToolkit")) settingsStore.CreateCollection("CRMToolkit");
                settingsStore.SetBoolean("CRMToolkit", m_service.GetSelectedProjectName(), true);

                var menuService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                var deployCommand = menuService.FindCommand(new CommandID(CommandSet, DeployCommandId));
                if (deployCommand != null) deployCommand.Enabled = true;

                var initCommand = menuService.FindCommand(new CommandID(CommandSet, InitCommandId));
                if (initCommand != null) initCommand.Enabled = false;
            }
            catch (Exception ex)
            {
                m_service.LogMessage($"[EXCEPTION] => {ex.Message}", m_pane);
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
            }
        }

        private void LogProgress(object sender, string e) => m_service.LogMessage(e, m_pane);

        private void HandleMenuItemChange(object sender, EventArgs e) { return; }
    }
}
