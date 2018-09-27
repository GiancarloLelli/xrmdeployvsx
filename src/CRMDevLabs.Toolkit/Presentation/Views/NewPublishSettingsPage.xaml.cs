using CRMDevLabs.Toolkit.Presentation.ViewModels;
using CRMDevLabs.Toolkit.Services;
using CRMDevLabs.Toolkit.Telemetry;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows;

namespace CRMDevLabs.Toolkit.Presentation.Views
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

        private void ClickForNavigationOnPatHowTo(object sender, RoutedEventArgs e)
        {
            (DataContext as NewPublishSettingsPageViewModel)?.NavigateToPatGuide.Execute(null);
        }

        private void CancelPublish(object sender, RoutedEventArgs e) => Close();
    }
}
