using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Xrm.Deploy.Vsix.Helpers;
using XRM.Deploy.Vsix.Models;
using XRM.Deploy.Vsix.Services;
using XRM.Telemetry;

namespace XRM.Deploy.Vsix.ViewModels
{
    internal class NewPublishSettingsPageViewModel : ViewModelBase
    {
        public RelayCommand<DialogWindow> SaveConfigurationCommand { get; set; }

        public DeployConfigurationModelFacade Configuration { get; set; }

        internal string FilePath { get; set; }

        private readonly DteService m_service;
        private readonly TelemetryWrapper m_telemetry;

        internal NewPublishSettingsPageViewModel(DteService service, TelemetryWrapper telemetry)
        {
            SaveConfigurationCommand = new RelayCommand<DialogWindow>((w) => Save(w));
            Configuration = new DeployConfigurationModelFacade();
            m_service = service;
            m_telemetry = telemetry;
        }

        private void Save(DialogWindow window)
        {
            var panelGuid = new Guid("A8E3D03E-28C9-4900-BD48-CEEDEC35E7E6");

            try
            {
                List<ValidationResult> results = new List<ValidationResult>();
                var validation = Validator.TryValidateObject(Configuration, new ValidationContext(Configuration), results);

                if (!validation)
                {
                    results.ForEach((error) => m_service.LogMessage($"[ERR] => {error.ErrorMessage}", panelGuid));
                    return;
                }

                var xmlDump = XmlObjectsHelper.Serialize(Configuration);
                var fullPath = $"{m_service.PropertiesDirectory}\\AvanadeToolkit.publishSettings";
                File.WriteAllText(fullPath, xmlDump);
                FilePath = fullPath;

                window.Close();
            }
            catch (Exception ex)
            {
                m_service.LogMessage($"[EXCEPTION] => {ex.Message}", panelGuid);
                m_telemetry.Instance.TrackExceptionWithCustomMetrics(ex, m_service.Version);
            }
        }
    }
}
