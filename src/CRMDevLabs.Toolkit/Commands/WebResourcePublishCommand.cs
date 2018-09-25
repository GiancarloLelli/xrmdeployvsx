using CRMDevLabs.Toolkit.Common.Serialization;
using CRMDevLabs.Toolkit.Common.Telemetry;
using CRMDevLabs.Toolkit.Feature.WebResource;
using CRMDevLabs.Toolkit.Models.Package;
using CRMDevLabs.Toolkit.Models.Telemetry;
using CRMDevLabs.Toolkit.Presentation.Models;
using CRMDevLabs.Toolkit.Presentation.ViewModels;
using CRMDevLabs.Toolkit.Presentation.Views;
using CRMDevLabs.Toolkit.Services;
using CRMDevLabs.Toolkit.Telemetry;
using CRMDevLabs.Toolkit.Xrm.Artifacts;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Threading;
using Async = System.Threading.Tasks;

namespace CRMDevLabs.Toolkit.Commands
{
    internal sealed class WebResourcePublishCommand
    {
        private Guid m_pane;
        private CancellationToken m_token;

        private readonly AsyncPackage m_package;
        private readonly DteService m_service;
        private readonly TelemetryWrapper m_telemetry;
        public const int InitCommandId = 0x1023;
        public const int DeployCommandId = 0x1024;
        public const int SingleDeployCommandId = 0x1025;
        private const string SETTINGS_STORE = "CRMToolkit";

        public static readonly Guid CommandSet = new Guid("df1214b3-0c72-4b2e-b906-c5f469fbe3bc");


        private WebResourcePublishCommand(AsyncPackage package, OleMenuCommandService commandService, IVsSettingsManager vsSettingsManager)
        {
            m_token = new CancellationToken();
            m_pane = new Guid("A8E3D03E-28C9-4900-BD48-CEEDEC35E7E6");
            m_service = new DteService(vsSettingsManager);
            m_package = package ?? throw new ArgumentNullException(nameof(package));
            m_telemetry = new TelemetryWrapper(m_service.Version, VersionHelper.GetVersionFromManifest());

            if (commandService != null)
            {
                var deployMenuCommandID = new CommandID(CommandSet, DeployCommandId);
                var deployMenuItem = new OleMenuCommand(DeployMenuItemCallback, deployMenuCommandID);
                deployMenuItem.BeforeQueryStatus += HandleDeployMenuState;
                commandService.AddCommand(deployMenuItem);

                var initializeMenuCommandId = new CommandID(CommandSet, InitCommandId);
                var initMenuItem = new OleMenuCommand(InitMenuItemCallback, initializeMenuCommandId);
                initMenuItem.BeforeQueryStatus += HandleInitMenuState;
                commandService.AddCommand(initMenuItem);

                var singeDeplotMenuCommandId = new CommandID(CommandSet, SingleDeployCommandId);
                var initSingleDeploy = new OleMenuCommand(SingleDeployMenuItemCallback, singeDeplotMenuCommandId);
                initSingleDeploy.BeforeQueryStatus += HandleSingleDeployMenuState;
                commandService.AddCommand(initSingleDeploy);

                // Fix for enable/disable states
                HandleInitMenuState(initMenuItem, null);
                HandleDeployMenuState(deployMenuItem, null);
            }
        }

        private void HandleSingleDeployMenuState(object sender, EventArgs e)
        {
            var menu = sender as OleMenuCommand;
            var isSingleDeploy = menu.CommandID.ID == SingleDeployCommandId;
            if (!isSingleDeploy) return;

            var projectName = m_service.GetSelectedProjectName();
            var objectName = m_service.GetSelectedObjectName();
            var isDeployEnabled = m_service.ReadOnlySettings.GetBoolean(SETTINGS_STORE, projectName, false);

            var file = new FileInfo(objectName);
            menu.Enabled = isDeployEnabled && FileExtensionToCodeConverter.Convert(objectName) != 99;
            menu.Text = $"Deploy {file.Name} to CRM";
        }

        private void HandleInitMenuState(object sender, EventArgs e)
        {
            var menu = sender as OleMenuCommand;
            var isInit = menu.CommandID.ID == InitCommandId;
            if (!isInit) return;

            var projectName = m_service.GetSelectedProjectName();
            var isDeployEnabled = m_service.ReadOnlySettings.GetBoolean(SETTINGS_STORE, projectName, false);

            menu.Enabled = !isDeployEnabled;
        }

        private void HandleDeployMenuState(object sender, EventArgs e)
        {
            var menu = sender as OleMenuCommand;
            var isDeploy = menu.CommandID.ID == DeployCommandId;
            if (!isDeploy) return;

            var projectName = m_service.GetSelectedProjectName();
            var isDeployEnabled = m_service.ReadOnlySettings.GetBoolean(SETTINGS_STORE, projectName, false);

            menu.Enabled = isDeployEnabled;
        }

        private void DeployMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                var singleResourceName = e is SingleResourceEventArgs ? (e as SingleResourceEventArgs).File : null;
                var projectName = m_service.GetSelectedProjectNameForAnalytics();
                m_telemetry.TrackCustomEventWithCustomMetrics("Deploy Started", new MetricData("Project Name", projectName));

                var publishSettigsPath = m_service.GetPublishSettingsFilePathIfExist();
                var solutionPath = m_service.GetSolutionRootPath();
                var basePath = m_service.GetProjectBasePath();

                // Delete settings after breaking update
                var settingsKey = "IsConfigDeleted-3.0";
                if (!m_service.ReadOnlySettings.GetBoolean(SETTINGS_STORE, settingsKey, false) && !string.IsNullOrEmpty(publishSettigsPath))
                {
                    File.Delete(publishSettigsPath);
                    publishSettigsPath = string.Empty;
                    m_service.WritableSettings.SetBoolean(SETTINGS_STORE, settingsKey, true);
                }

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

                var task = Async.Task.Factory.StartNew(() =>
                {
                    orchestrator.Publish(deployConfiguration.InnerObject, m_telemetry, singleResourceName, projectName, solutionPath, basePath);
                    orchestrator.ReportProgress -= LogProgress;
                }, m_token, Async.TaskCreationOptions.None, Async.TaskScheduler.Current);
            }
            catch (Exception ex)
            {
                m_service.LogMessage($"[ERROR] => {ex.Message}", m_pane);
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
            }
        }

        private void InitMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                m_telemetry.TrackCustomEventWithCustomMetrics("Project Initialization", new MetricData("Project Name", m_service.GetSelectedProjectNameForAnalytics()));
                if (!m_service.WritableSettings.CollectionExists(SETTINGS_STORE)) m_service.WritableSettings.CreateCollection(SETTINGS_STORE);
                m_service.WritableSettings.SetBoolean(SETTINGS_STORE, m_service.GetSelectedProjectName(), true);
            }
            catch (Exception ex)
            {
                m_service.LogMessage($"[ERROR] => {ex.Message}", m_pane);
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
            }
        }

        private void SingleDeployMenuItemCallback(object sender, EventArgs e)
        {
            var fileInfo = new FileInfo(m_service.GetSelectedObjectName());
            DeployMenuItemCallback(sender, new SingleResourceEventArgs(fileInfo.Name));
        }

        private void LogProgress(object sender, string e) => m_service.LogMessage(e, m_pane);

        public static WebResourcePublishCommand Instance { get; private set; }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider { get { return this.m_package; } }

        public static async Async.Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            var settingsManager = await package.GetServiceAsync(typeof(SVsSettingsManager)) as IVsSettingsManager;
            Instance = new WebResourcePublishCommand(package, commandService, settingsManager);
        }
    }
}
