// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Extensions
{
    using Microsoft.WindowsAzure.Management.ServiceManagement.Helpers;
    using System;
    using System.Linq;
    using System.Management.Automation;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;
    using System.Xml.Linq;
    using Properties;
    using Utilities.Common;
    using WindowsAzure.Management.Utilities.CloudService;
    using WindowsAzure.ServiceManagement;

    public abstract class BaseAzureServiceExtensionCmdlet : ServiceManagementBaseCmdlet
    {
        protected const string PublicConfigStr = "PublicConfig";
        protected const string PrivateConfigStr = "PrivateConfig";
        protected const string ChangeConfigurationModeStr = "Auto";
        protected ExtensionManager ExtensionManager;
        protected string ExtensionNameSpace;
        protected string ExtensionType;
        protected XDocument PublicConfigurationXmlTemplate;
        protected XDocument PrivateConfigurationXmlTemplate;

        public BaseAzureServiceExtensionCmdlet()
            : base()
        {
        }

        public BaseAzureServiceExtensionCmdlet(IServiceManagement channel)
            : base()
        {
            Channel = channel;
        }

        public virtual string ServiceName
        {
            get;
            set;
        }

        public virtual string Slot
        {
            get;
            set;
        }

        public virtual string[] Role
        {
            get;
            set;
        }

        public virtual X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        public virtual string CertificateThumbprint
        {
            get;
            set;
        }

        public virtual string ThumbprintAlgorithm
        {
            get;
            set;
        }

        protected virtual Deployment Deployment
        {
            get;
            set;
        }

        protected virtual void ValidateParameters()
        {
        }

        protected void ValidateService()
        {
            string serviceName;
            ServiceSettings settings = General.GetDefaultSettings(General.TryGetServiceRootPath(CurrentPath()),
                ServiceName, null, null, null, null, CurrentSubscription.SubscriptionId, out serviceName);

            if (string.IsNullOrEmpty(serviceName))
            {
                throw new Exception(string.Format(Resources.ServiceExtensionCannotFindServiceName, ServiceName));
            }
            else
            {
                ServiceName = serviceName;
                if (!IsServiceAvailable(ServiceName))
                {
                    throw new Exception(string.Format(Resources.ServiceExtensionCannotFindServiceName, ServiceName));
                }
            }

            ExtensionManager = new ExtensionManager(Channel, CurrentSubscription.SubscriptionId, ServiceName);
        }

        protected void ValidateDeployment()
        {
            Slot = string.IsNullOrEmpty(Slot) ? DeploymentSlotType.Production : Slot;

            Deployment = Channel.GetDeploymentBySlot(CurrentSubscription.SubscriptionId, ServiceName, Slot);
            if (Deployment == null)
            {
                throw new Exception(string.Format(Resources.ServiceExtensionCannotFindDeployment, ServiceName, Slot));
            }

            if (Deployment.ExtensionConfiguration == null)
            {
                Deployment.ExtensionConfiguration = new ExtensionConfiguration
                {
                    AllRoles = new AllRoles(),
                    NamedRoles = new NamedRoles()
                };
            }
        }

        protected void ValidateRoles()
        {
            if (Role != null)
            {
                Role.ForEach(r => r = r == null ? r : r.Trim());
                Role = Role.Distinct().ToArray();

                foreach (string roleName in Role)
                {
                    if (Deployment.RoleList == null || !Deployment.RoleList.Any(r => r.RoleName == roleName))
                    {
                        throw new Exception(string.Format(Resources.ServiceExtensionCannotFindRole, roleName, Slot, ServiceName));
                    }

                    if (string.IsNullOrWhiteSpace(roleName))
                    {
                        throw new Exception(Resources.ServiceExtensionCannotFindRoleName);
                    }
                }
            }
        }

        protected void ValidateThumbprint(bool uploadCert)
        {
            if (X509Certificate != null)
            {
                var operationDescription = string.Format(Resources.ServiceExtensionUploadingCertificate, CommandRuntime, X509Certificate.Thumbprint);
                if (uploadCert)
                {
                    ExecuteClientActionInOCS(null, operationDescription, s => this.Channel.AddCertificates(s, this.ServiceName, CertUtils.Create(X509Certificate)));
                }
                CertificateThumbprint = X509Certificate.Thumbprint;
            }
            ThumbprintAlgorithm = ThumbprintAlgorithm == null ? "" : ThumbprintAlgorithm;
            CertificateThumbprint = CertificateThumbprint == null ? "" : CertificateThumbprint;
        }

        protected virtual bool IsServiceAvailable(string serviceName)
        {
            // Check that cloud service exists
            bool found = false;
            InvokeInOperationContext(() =>
            {
                this.RetryCall(s => found = !Channel.IsDNSAvailable(CurrentSubscription.SubscriptionId, serviceName).Result);
            });
            return found;
        }

        private static string GetConfigValue(string text, string elem)
        {
            XDocument config = XDocument.Parse(text);
            var result = from d in config.Descendants()
                         where d.Name.LocalName == elem
                         select d.Descendants().Any() ? d.ToString() : d.Value;
            return result.FirstOrDefault();
        }

        protected string GetPublicConfigValue(HostedServiceExtension ext, string elem)
        {
            return ext == null ? "" : GetConfigValue(ext.PublicConfiguration, elem);
        }

        protected void ChangeDeployment(ExtensionConfiguration extConfig)
        {
            ChangeConfigurationInput changeConfigInput = new ChangeConfigurationInput
            {
                Configuration = Deployment.Configuration,
                ExtendedProperties = Deployment.ExtendedProperties,
                ExtensionConfiguration = Deployment.ExtensionConfiguration = extConfig,
                Mode = ChangeConfigurationModeStr,
                TreatWarningsAsError = false
            };
            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => Channel.ChangeConfigurationBySlot(s, ServiceName, Slot, changeConfigInput));
        }
    }
}
