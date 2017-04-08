using GalaSoft.MvvmLight;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using XRM.Deploy.Core.Models;

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

        #region CRM Connection Settings
        [Required]
        public string ConnectionString
        {
            get { return m_model.CrmSettings.Discovery; }
            set
            {
                m_model.CrmSettings.Discovery = value;
                RaisePropertyChanged(() => ConnectionString);
            }
        }

        [Required]
        public string CrmUsername
        {
            get { return m_model.CrmSettings.Username; }
            set
            {
                m_model.CrmSettings.Username = value;
                RaisePropertyChanged(() => CrmUsername);
            }
        }

        [Required]
        public string CrmPassword
        {
            get { return m_model.CrmSettings.Password; }
            set
            {
                m_model.CrmSettings.Password = value;
                RaisePropertyChanged(() => CrmPassword);
            }
        }

        [Required]
        public string CrmDomain
        {
            get { return m_model.CrmSettings.Domain; }
            set
            {
                m_model.CrmSettings.Domain = value;
                RaisePropertyChanged(() => CrmDomain);
            }
        }

        [Required]
        public string CrmOrganization
        {
            get { return m_model.CrmSettings.OrganizationName; }
            set
            {
                m_model.CrmSettings.OrganizationName = value;
                RaisePropertyChanged(() => CrmOrganization);
            }
        }
        #endregion

        #region Source Control Settings
        [Required, DataType(DataType.Url)]
        public string TfsProjectCollectionUri
        {
            get { return m_model.SourceControlSettings.TFSCollectionUrl; }
            set
            {
                m_model.SourceControlSettings.TFSCollectionUrl = value;
                RaisePropertyChanged(() => TfsProjectCollectionUri);
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

        public bool IsOnPrem
        {
            get { return m_model.SourceControlSettings.IsOnPrem; }
            set
            {
                m_model.SourceControlSettings.IsOnPrem = value;
                RaisePropertyChanged(() => IsOnPrem);
                CleanOnlineData();
            }
        }

        public bool IsOnline
        {
            get { return m_model.SourceControlSettings.IsOnline; }
            set
            {
                m_model.SourceControlSettings.IsOnline = value;
                RaisePropertyChanged(() => IsOnline);
                CleanOnPremData();
            }
        }

        public string Domain
        {
            get { return m_model.SourceControlSettings.Domain; }
            set
            {
                m_model.SourceControlSettings.Domain = value;
                RaisePropertyChanged(() => Domain);
            }
        }

        public string User
        {
            get { return m_model.SourceControlSettings.User; }
            set
            {
                m_model.SourceControlSettings.User = value;
                RaisePropertyChanged(() => User);
            }
        }

        public string Password
        {
            get { return m_model.SourceControlSettings.Password; }
            set
            {
                m_model.SourceControlSettings.Password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        public string Pat
        {
            get { return m_model.SourceControlSettings.Pat; }
            set
            {
                m_model.SourceControlSettings.Pat = value;
                RaisePropertyChanged(() => Pat);
            }
        }
        #endregion

        #region General deploy settings
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
        #endregion

        private void CleanOnPremData()
        {
            Password = string.Empty;
            Domain = string.Empty;
        }

        private void CleanOnlineData()
        {
            Pat = string.Empty;
        }
    }
}
