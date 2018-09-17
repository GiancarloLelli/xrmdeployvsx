using CRMDevLabs.Toolkit.Common.Serialization;
using CRMDevLabs.Toolkit.Telemetry;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using XRM.Deploy.Vsix.Models;
using XRM.Deploy.Vsix.Services;

namespace XRM.Deploy.Vsix.ViewModels
{
    public class NewPublishSettingsPageViewModel : ViewModelBase
    {
        private readonly DteService m_service;
        private readonly TelemetryWrapper m_telemetry;

        public RelayCommand<DialogWindow> SaveConfigurationCommand { get; set; }

        public RelayCommand NavigateToPatGuide { get; set; }

        public DeployConfigurationModelFacade Configuration { get; set; }

        public string FilePath { get; set; }

        public NewPublishSettingsPageViewModel(DteService service, TelemetryWrapper telemetry)
        {
            SaveConfigurationCommand = new RelayCommand<DialogWindow>((w) => Save(w));
            NavigateToPatGuide = new RelayCommand(() => NavigateToUrl());
            Configuration = new DeployConfigurationModelFacade();
            m_service = service;
            m_telemetry = telemetry;
        }

        private void NavigateToUrl() => Process.Start("https://www.visualstudio.com/en-us/docs/setup-admin/team-services/use-personal-access-tokens-to-authenticate");

        private void Save(DialogWindow window)
        {
            var panelGuid = new Guid("A8E3D03E-28C9-4900-BD48-CEEDEC35E7E6");

            try
            {
                string customError = string.Empty;
                List<ValidationResult> results = new List<ValidationResult>();
                var validation = Validator.TryValidateObject(Configuration, new ValidationContext(Configuration), results);

                if (!validation || !Configuration.AreCustomFieldsValid(out customError))
                {
                    results.ForEach((error) => m_service.LogMessage($"[EXCEPTION] => {error.ErrorMessage}", panelGuid));
                    m_service.LogMessage($"[EXCEPTION] => {customError}", panelGuid);
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
                m_telemetry.TrackExceptionWithCustomMetrics(ex);
            }
        }
    }
}
