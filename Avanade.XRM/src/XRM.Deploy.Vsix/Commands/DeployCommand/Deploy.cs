using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace XRM.Deploy.Vsix.Commands.DeployCommand
{
    internal sealed class Deploy
    {
        private readonly Package m_package;

        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("222af809-1598-498f-a9d7-6b130d420527");

        public static Deploy Instance { get; private set; }

        private IServiceProvider ServiceProvider { get { return m_package; } }

        private Deploy(Package package)
        {
            if (package == null) throw new ArgumentNullException("Package");
            m_package = package;

            OleMenuCommandService commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);

                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += MenuItemVisibilityEnablement;

                commandService.AddCommand(menuItem);
            }
        }

        private void MenuItemVisibilityEnablement(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void Initialize(Package package)
        {
            Instance = new Deploy(package);
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {

        }
    }
}
