using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Xrm.Deploy.Vsix.Helpers;
using XRM.Deploy.Vsix.Models;
using XRM.Deploy.Vsix.Services;

namespace XRM.Deploy.Vsix.ViewModels
{
    internal class NewPublishSettingsPageViewModel : ViewModelBase
    {
        public RelayCommand SaveConfigurationCommand { get; set; }

        public DeployConfigurationModelFacade Configuration { get; set; }

        internal string FilePath { get; set; }

        private readonly DteService m_service;

        internal NewPublishSettingsPageViewModel()
        {
            SaveConfigurationCommand = new RelayCommand(() => Save());
            Configuration = new DeployConfigurationModelFacade();
            m_service = new DteService();
        }

        private void Save()
        {
            List<ValidationResult> results = new List<ValidationResult>();
            var validation = Validator.TryValidateObject(Configuration, new ValidationContext(Configuration), results);

            if (!validation)
            {
                results.ForEach((error) => m_service.LogMessage($"[ERR] => {error.ErrorMessage}", new Guid("A8E3D03E-28C9-4900-BD48-CEEDEC35E7E6")));
                return;
            }

            var propertiesProjectpath = m_service.GetProprtiesFolderPath();
            var xmlDump = XmlObjectsHelper.Serialize(Configuration);

            var fullPath = $"{propertiesProjectpath}AvanadeToolkit.publishSettings";
            File.WriteAllText(fullPath, xmlDump);
            FilePath = fullPath;
        }
    }
}
