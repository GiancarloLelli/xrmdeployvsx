using Microsoft.VisualStudio.PlatformUI;
using XRM.Deploy.Vsix.ViewModels;

namespace XRM.Deploy.Vsix.Views
{
    public partial class NewPublishSettingsPage : DialogWindow
    {
        public string ConfigurationFilePath { get; set; }

        public NewPublishSettingsPage()
        {
            InitializeComponent();
            DataContext = new NewPublishSettingsPageViewModel();
        }

        private void CancelPublish(object sender, System.Windows.RoutedEventArgs e) => Close();
    }
}
