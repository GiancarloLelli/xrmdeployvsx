using CRMDevLabs.Toolkit.Models.Xrm;
using GalaSoft.MvvmLight;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace CRMDevLabs.Toolkit.Presentation.Models
{
    [Serializable]
    public class DeployConfigurationModelFacade : ObservableObject
    {
        [XmlIgnore]
        private DeployConfigurationModel m_model;

        [XmlIgnore]
        public DeployConfigurationModel InnerObject
        {
            get
            {
                return m_model;
            }
        }

        public DeployConfigurationModelFacade()
        {
            m_model = new DeployConfigurationModel();
        }

        [Required]
        public string ConnectionString
        {
            get { return m_model.DynamicsSettings.FullString; }
            set
            {
                m_model.DynamicsSettings.FullString = value;
                RaisePropertyChanged(() => ConnectionString);
            }
        }

        [Required]
        public string Branch
        {
            get { return m_model.Branch; }
            set
            {
                m_model.Branch = value;
                RaisePropertyChanged(() => Branch);
            }
        }

        [Required]
        public string Username
        {
            get { return m_model.Username; }
            set
            {
                m_model.Username = value;
                RaisePropertyChanged(() => Username);
            }
        }

        [Required]
        public string Password
        {
            get { return m_model.Password; }
            set
            {
                m_model.Password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        public bool CheckInEnabled
        {
            get { return m_model.CheckInEnabled; }
            set
            {
                m_model.CheckInEnabled = value;
                RaisePropertyChanged(() => CheckInEnabled);
            }
        }

        [Required, RegularExpression("^[a-zA-z]_$")]
        public string Prefix
        {
            get { return m_model.Prefix; }
            set
            {
                m_model.Prefix = value;
                RaisePropertyChanged(() => Prefix);
            }
        }

        [Required]
        public string Solution
        {
            get { return m_model.Solution; }
            set
            {
                m_model.Solution = value;
                RaisePropertyChanged(() => Solution);
            }
        }
    }
}
