using Microsoft.VisualStudio.PlatformUI;
using XRM.Deploy.Vsix.Services;
using XRM.Deploy.Vsix.ViewModels;
using XRM.Telemetry;

namespace XRM.Deploy.Vsix.Views
{
    public partial class NewPublishSettingsPage : DialogWindow
    {
        public string ConfigurationFilePath { get; set; }

        public NewPublishSettingsPage()
        {
            InitializeComponent();
        }

        public NewPublishSettingsPage(DteService dteService, TelemetryWrapper telemetry)
        {
            InitializeComponent();
            DataContext = new NewPublishSettingsPageViewModel(dteService, telemetry);
        }

        private void CancelPublish(object sender, System.Windows.RoutedEventArgs e) => Close();
    }
}
