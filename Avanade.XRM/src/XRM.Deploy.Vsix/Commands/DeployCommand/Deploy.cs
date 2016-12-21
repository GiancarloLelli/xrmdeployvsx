using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Xrm.Deploy.Vsix.Helpers;
using XRM.Deploy.Core;
using XRM.Deploy.Vsix.Models;
using XRM.Deploy.Vsix.Services;
using XRM.Deploy.Vsix.ViewModels;
using XRM.Deploy.Vsix.Views;
using XRM.Telemetry;

namespace XRM.Deploy.Vsix.Commands.DeployCommand
{
    internal sealed class Deploy
    {
        private Guid m_pane;
        private readonly Package m_package;
        private readonly DteService m_service;
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("222af809-1598-498f-a9d7-6b130d420527");

        public static Deploy Instance { get; private set; }

        private IServiceProvider ServiceProvider { get { return m_package; } }

        public static void Initialize(Package package) => Instance = new Deploy(package);

        private Deploy(Package package)
        {
            if (package == null) throw new ArgumentNullException("Package");
            m_package = package;
            m_pane = new Guid("A8E3D03E-28C9-4900-BD48-CEEDEC35E7E6");
            m_service = new DteService();

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                var publishSettigsPath = m_service.GetPublishSettingsFilePathIfExist();
                if (string.IsNullOrEmpty(publishSettigsPath))
                {
                    var dialog = new NewPublishSettingsPage(m_service);
                    dialog.ShowDialog();
                    publishSettigsPath = (dialog.DataContext as NewPublishSettingsPageViewModel)?.FilePath;
                }

                // No valid configuration found or provided
                if (string.IsNullOrEmpty(publishSettigsPath)) return;

                var deployConfiguration = XmlObjectsHelper.Deserialize<DeployConfigurationModelFacade>(publishSettigsPath);
                var orchestrator = new PublishOrchestrator();
                orchestrator.ReportProgress += LogProgress;
                orchestrator.Publish(deployConfiguration.InnerObject);
                orchestrator.ReportProgress -= LogProgress;
            }
            catch (Exception ex)
            {
                m_service.LogMessage($"[EXCEPTION] => {ex.Message}", m_pane);
                TelemetryWrapper.Instance.TrackExceptionWithCustomMetrics(ex, m_service.Version);
            }
        }

        private void LogProgress(object sender, string e) => m_service.LogMessage(e, m_pane);
    }
}
