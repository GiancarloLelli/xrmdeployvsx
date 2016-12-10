using GalaSoft.MvvmLight;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Xrm.Deploy.Core.Models;

namespace XRM.Deploy.Vsix.Models
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

        [Required, DataType(DataType.Url)]
        public string TfsProjectCollectionUri
        {
            get { return m_model.TFSCollectionUrl; }
            set
            {
                m_model.TFSCollectionUrl = value;
                RaisePropertyChanged(() => TfsProjectCollectionUri);
            }
        }

        [Required]
        public string ConnectionString
        {
            get { return m_model.CRMConnectionString; }
            set
            {
                m_model.CRMConnectionString = value;
                RaisePropertyChanged(() => ConnectionString);
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

        [Required, RegularExpression("^[a-zA-z]{3}_$")]
        public string Prefix
        {
            get { return m_model.Prefix; }
            set
            {
                m_model.Prefix = value;
                RaisePropertyChanged(() => Prefix);
            }
        }

        public string Domain
        {
            get { return m_model.Domain; }
            set
            {
                m_model.Domain = value;
                RaisePropertyChanged(() => Domain);
            }
        }

        public string User
        {
            get { return m_model.User; }
            set
            {
                m_model.User = value;
                RaisePropertyChanged(() => User);
            }
        }

        public string Password
        {
            get { return m_model.Password; }
            set
            {
                m_model.Password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        public string Workspace
        {
            get { return m_model.Workspace; }
            set
            {
                m_model.Workspace = value;
                RaisePropertyChanged(() => Workspace);
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
    }
}
