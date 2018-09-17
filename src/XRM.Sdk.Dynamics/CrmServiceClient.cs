// =====================================================================
//  This file is part of the Microsoft Dynamics CRM SDK code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
// =====================================================================

using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using System;
using System.Net;
using System.ServiceModel.Description;
using XRM.Sdk.Dynamics.Models;

namespace XRM.Sdk.Dynamics
{
    public class CrmServiceClient
    {
        private readonly string m_discovery;
        private readonly string m_organization;
        private readonly string m_username;
        private readonly string m_password;
        private readonly string m_domain;

        public CrmServiceClient(string discoveryUrl, string orgUniqueName, string user, string password, string domain)
        {
            m_discovery = discoveryUrl;
            m_organization = orgUniqueName;
            m_username = user;
            m_password = password;
            m_domain = domain;
        }

        public CrmServiceClient(CrmConnectionSettings connectionSettings)
        {
            m_discovery = connectionSettings.Discovery;
            m_organization = connectionSettings.OrganizationName;
            m_username = connectionSettings.Username;
            m_password = connectionSettings.Password;
            m_domain = connectionSettings.Domain;
        }

        public OrganizationServiceProxy OrganizationServiceFactory()
        {
            OrganizationServiceProxy organizationService = null;

            var serviceManagement = ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(new Uri(m_discovery));
            var endpointType = serviceManagement.AuthenticationType;
            var authCredentials = GetCredentials(serviceManagement, endpointType);

            // Get the discovery service proxy.
            string organizationUri = string.Empty;
            using (var discoveryProxy = GetProxy<IDiscoveryService, DiscoveryServiceProxy>(serviceManagement, authCredentials))
            {
                if (discoveryProxy != null)
                {
                    var orgs = DiscoverOrganizations(discoveryProxy);
                    organizationUri = FindOrganization(m_organization, orgs.ToArray()).Endpoints[EndpointType.OrganizationService];
                }
            }

            // Get the discovery OrganizationService.
            if (!string.IsNullOrWhiteSpace(organizationUri))
            {
                var orgServiceManagement = ServiceConfigurationFactory.CreateManagement<IOrganizationService>(new Uri(organizationUri));
                var credentials = GetCredentials(orgServiceManagement, endpointType);
                organizationService = GetProxy<IOrganizationService, OrganizationServiceProxy>(orgServiceManagement, credentials);
            }

            return organizationService;
        }

        private AuthenticationCredentials GetCredentials<TService>(IServiceManagement<TService> service, AuthenticationProviderType endpointType)
        {
            AuthenticationCredentials authCredentials = new AuthenticationCredentials();

            switch (endpointType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    authCredentials.ClientCredentials.Windows.ClientCredential = new NetworkCredential(m_username, m_password, m_domain);
                    break;
                case AuthenticationProviderType.LiveId:
                    authCredentials.ClientCredentials.UserName.UserName = m_username;
                    authCredentials.ClientCredentials.UserName.Password = m_password;
                    authCredentials.SupportingCredentials = new AuthenticationCredentials();
                    authCredentials.SupportingCredentials.ClientCredentials = DeviceIdManager.LoadOrRegisterDevice();
                    break;
                default:
                    authCredentials.ClientCredentials.UserName.UserName = m_username;
                    authCredentials.ClientCredentials.UserName.Password = m_password;
                    if (endpointType == AuthenticationProviderType.OnlineFederation)
                    {
                        IdentityProvider provider = service.GetIdentityProvider(authCredentials.ClientCredentials.UserName.UserName);
                        if (provider != null && provider.IdentityProviderType == IdentityProviderType.LiveId)
                        {
                            authCredentials.SupportingCredentials = new AuthenticationCredentials();
                            authCredentials.SupportingCredentials.ClientCredentials = DeviceIdManager.LoadOrRegisterDevice();
                        }
                    }
                    break;
            }

            return authCredentials;
        }

        private OrganizationDetailCollection DiscoverOrganizations(IDiscoveryService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            RetrieveOrganizationsRequest orgRequest = new RetrieveOrganizationsRequest();
            RetrieveOrganizationsResponse orgResponse = (RetrieveOrganizationsResponse)service.Execute(orgRequest);
            return orgResponse.Details;
        }

        private OrganizationDetail FindOrganization(string orgUniqueName, OrganizationDetail[] orgDetails)
        {
            if (string.IsNullOrWhiteSpace(orgUniqueName)) throw new ArgumentNullException("orgUniqueName");
            if (orgDetails == null) throw new ArgumentNullException("orgDetails");
            OrganizationDetail orgDetail = null;

            foreach (OrganizationDetail detail in orgDetails)
            {
                if (string.Compare(detail.UniqueName, orgUniqueName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    orgDetail = detail;
                    break;
                }
            }

            return orgDetail;
        }

        private TProxy GetProxy<TService, TProxy>(IServiceManagement<TService> serviceManagement, AuthenticationCredentials authCredentials) where TService : class where TProxy : ServiceProxy<TService>
        {
            Type classType = typeof(TProxy);

            if (serviceManagement.AuthenticationType != AuthenticationProviderType.ActiveDirectory)
            {
                AuthenticationCredentials tokenCredentials = serviceManagement.Authenticate(authCredentials);
                // Obtain discovery/organization service proxy for Federated, LiveId and OnlineFederated environments. 
                // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and SecurityTokenResponse.
                return (TProxy)classType
                    .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(SecurityTokenResponse) })
                    .Invoke(new object[] { serviceManagement, tokenCredentials.SecurityTokenResponse });
            }

            // Obtain discovery/organization service proxy for ActiveDirectory environment.
            // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and ClientCredentials.
            return (TProxy)classType
                .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(ClientCredentials) })
                .Invoke(new object[] { serviceManagement, authCredentials.ClientCredentials });
        }
    }
}