using CRMDevLabs.Toolkit.Common.Serialization;
using CRMDevLabs.Toolkit.Presentation.Models;
using CRMDevLabs.Toolkit.Services;
using CRMDevLabs.Toolkit.Telemetry;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CRMDevLabs.Toolkit.Presentation.ViewModels
{
    public class NewPublishSettingsPageViewModel : ViewModelBase
    {
        private readonly DteService m_service;
        private readonly TelemetryWrapper m_telemetry;

        public RelayCommand<DialogWindow> SaveConfigurationCommand { get; set; }

        public DeployConfigurationModelFacade Configuration { get; set; }

        public string FilePath { get; set; }

        public NewPublishSettingsPageViewModel(DteService service, TelemetryWrapper telemetry)
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
                string customError = string.Empty;
                List<ValidationResult> results = new List<ValidationResult>();
                var validation = Validator.TryValidateObject(Configuration, new ValidationContext(Configuration), results);

                if (!validation)
                {
                    results.ForEach((error) => m_service.LogMessage($"[ERROR] => {error.ErrorMessage}", panelGuid));
                    m_service.LogMessage($"[ERROR] => {customError}", panelGuid);
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
                m_service.LogMessage($"[ERROR] => {ex.Message}", panelGuid);
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
            }
        }
    }
}
