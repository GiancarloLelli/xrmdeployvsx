using Microsoft.VisualStudio.PlatformUI;
using XRM.Deploy.Vsix.Services;
using XRM.Deploy.Vsix.ViewModels;

namespace XRM.Deploy.Vsix.Views
{
    public partial class NewPublishSettingsPage : DialogWindow
    {
        public string ConfigurationFilePath { get; set; }

        public NewPublishSettingsPage()
        {
            InitializeComponent();
        }

        public NewPublishSettingsPage(DteService dteService)
        {
            InitializeComponent();
            DataContext = new NewPublishSettingsPageViewModel(dteService);
        }

        private void CancelPublish(object sender, System.Windows.RoutedEventArgs e) => Close();
    }
}
