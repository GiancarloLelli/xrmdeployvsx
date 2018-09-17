using CRMDevLabs.Toolkit.Models.Xrm;
using GalaSoft.MvvmLight;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

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

        [Required]
        public string Pat
        {
            get { return m_model.SourceControlSettings.Pat; }
            set
            {
                m_model.SourceControlSettings.Pat = value;
                RaisePropertyChanged(() => Pat);
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
