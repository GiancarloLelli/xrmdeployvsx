using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using Xrm.Deploy.Core.Models;
using XRM.Deploy.Core;

namespace XRM.Deploy.Vsix.Commands.DeployCommand
{
    internal sealed class Deploy
    {
        private Guid m_pane;
        private readonly Package m_package;
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
            // Check if config file exist, if not show window and create one.
            LogProgress(null, "Hello, World!");
            return;

            var orchestrator = new PublishOrchestrator();
            orchestrator.ReportProgress += LogProgress;
            orchestrator.Publish(new DeployConfigurationModel());
            orchestrator.ReportProgress -= LogProgress;
        }

        private void LogProgress(object sender, string e)
        {
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outWindow != null)
            {
                IVsOutputWindowPane customPane = null;
                outWindow.GetPane(ref m_pane, out customPane);

                if (customPane == null)
                {
                    outWindow.CreatePane(ref m_pane, "Avanade CRM Toolkit - Publish Output", 1, 1);
                    outWindow.GetPane(ref m_pane, out customPane);
                }

                if (customPane != null)
                {
                    customPane?.OutputString(string.Concat(e, Environment.NewLine));
                    customPane?.Activate();
                }
            }
        }
    }
}
