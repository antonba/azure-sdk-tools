// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.Text;
    using WindowsAzure.ServiceManagement;

    public class ExtensionManager
    {
        public const int ExtensionIdLiveCycleCount = 2;
        private const string ExtensionIdTemplate = "{0}-{1}-{2}-Ext-{3}";
        private const string DefaultAllRolesNameStr = "Default";
        private const string ExtensionCertificateSubject = "DC=Windows Azure Service Management for Extensions";
        private const string ThumbprintAlgorithmStr = "sha1";

        public IServiceManagement Channel
        {
            get;
            private set;
        }

        public string SubscriptionId
        {
            get;
            private set;
        }

        public string ServiceName
        {
            get;
            private set;
        }

        public ExtensionManager(IServiceManagement channel, string subscriptionId, string serviceName)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            Channel = channel;

            if (string.IsNullOrEmpty(subscriptionId))
            {
                throw new ArgumentNullException("subscriptionId");
            }
            SubscriptionId = subscriptionId;

            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException("serviceName");
            }
            ServiceName = serviceName;
        }

        public HostedServiceExtension GetExtension(string extensionId)
        {
            return Channel.ListHostedServiceExtensions(SubscriptionId, ServiceName).Find(e => e.Id == extensionId);
        }

        public void DeleteExtension(string extensionId)
        {
            Channel.DeleteHostedServiceExtension(SubscriptionId, ServiceName, extensionId);
        }

        public void AddExtension(HostedServiceExtensionInput extension)
        {
            Channel.AddHostedServiceExtension(SubscriptionId, ServiceName, extension);
        }

        public bool CheckExtensionType(string extensionId, string nameSpace, string type)
        {
            if (!string.IsNullOrEmpty(extensionId))
            {
                HostedServiceExtension extension = GetExtension(extensionId);
                return extension != null && extension.ProviderNameSpace == nameSpace && extension.Type == type;
            }
            return false;
        }

        public ExtensionConfigurationBuilder GetBuilder()
        {
            return new ExtensionConfigurationBuilder(this);
        }

        public ExtensionConfigurationBuilder GetBuilder(ExtensionConfiguration config)
        {
            return GetBuilder().Add(config);
        }

        public void Uninstall(string nameSpace, string type, string deploymentSlot)
        {
            var slotList = new string[] { DeploymentSlotType.Production, DeploymentSlotType.Staging };
            List<Deployment> deploymentList = new List<Deployment>();
            foreach (var slot in slotList)
            {
                Deployment currentDeployment = null;
                using (new OperationContextScope(Channel.ToContextChannel()))
                {
                    try
                    {
                        currentDeployment = Channel.GetDeploymentBySlot(SubscriptionId, ServiceName, slot);
                    }
                    catch (Exception)
                    {
                        // Do nothing
                    }
                }
                if (currentDeployment != null)
                {
                    deploymentList.Add(currentDeployment);
                }
            }

            var roleList = deploymentList.First(d => d.DeploymentSlot == deploymentSlot);
            Channel.ListHostedServiceExtensions(SubscriptionId, ServiceName).ForEach(e =>
            {
                if (e.ProviderNameSpace == nameSpace && e.Type == type)
                {
                    bool found = deploymentList.Any(d =>
                    {
                        ExtensionConfigurationBuilder builder = GetBuilder(d.ExtensionConfiguration);
                        return builder.ExistAny(e.Id);
                    });

                    if (!found)
                    {
                        Channel.DeleteHostedServiceExtension(SubscriptionId, ServiceName, e.Id);
                    }
                }
            });
        }

        public string GetExtensionId(string roleName, string type, string slot, int index)
        {
            return string.Format(ExtensionIdTemplate, roleName, type, slot, index);
        }

        private void GetExtensionThumbprintAndAlgorithm(List<HostedServiceExtension> extensionList, string extensionId, ref string thumbprint, ref string thumbprintAlgorithm)
        {
            var existingExtension = extensionList.Find(e => e.Id == extensionId);
            if (existingExtension != null)
            {
                thumbprint = existingExtension.Thumbprint;
                thumbprintAlgorithm = existingExtension.ThumbprintAlgorithm;
            }
            else if (extensionList.Any())
            {
                thumbprint = extensionList.First().Thumbprint;
                thumbprintAlgorithm = extensionList.First().ThumbprintAlgorithm;
            }

            string extThumbprint = thumbprint;
            var certList = Channel.ListCertificates(SubscriptionId, ServiceName);
            var cert = certList.Find(c =>
            {
                if (string.IsNullOrWhiteSpace(extThumbprint))
                {
                    return true;
                }
                byte[] bytes = Encoding.ASCII.GetBytes(c.Data);
                X509Certificate2 x509cert = null;
                try
                {
                    x509cert = new X509Certificate2(bytes);
                }
                catch (CryptographicException)
                {
                    // Do nothing
                }
                return x509cert != null && ExtensionCertificateSubject.Equals(x509cert.Subject);
            });

            if (cert != null)
            {
                thumbprint = cert.Thumbprint;
                thumbprintAlgorithm = cert.ThumbprintAlgorithm;
            }
        }

        public ExtensionConfiguration InstallExtension(ExtensionConfigurationInput context, string slot, ExtensionConfiguration extConfig)
        {
            ExtensionConfigurationBuilder builder = GetBuilder(extConfig);
            foreach (ExtensionRole r in context.Roles)
            {
                string roleName = r.RoleType == ExtensionRoleType.AllRoles ? DefaultAllRolesNameStr : r.RoleName;

                var extensionIds = (from index in Enumerable.Range(0, ExtensionIdLiveCycleCount)
                                    select GetExtensionId(roleName, context.Type, slot, index)).ToList();

                string availableId = (from extensionId in extensionIds
                                      where !builder.ExistAny(extensionId)
                                      select extensionId).FirstOrDefault();

                var extensionList = (from id in extensionIds
                                     let e = GetExtension(id)
                                     where e != null
                                     select e).ToList();

                string thumbprint = context.CertificateThumbprint;
                string thumbprintAlgorithm = context.ThumbprintAlgorithm;

                if (context.X509Certificate != null)
                {
                    thumbprint = context.X509Certificate.Thumbprint;
                }
                else
                {
                    GetExtensionThumbprintAndAlgorithm(extensionList, availableId, ref thumbprint, ref thumbprintAlgorithm);
                }

                context.CertificateThumbprint = string.IsNullOrWhiteSpace(context.CertificateThumbprint) ? thumbprint : context.CertificateThumbprint;
                context.ThumbprintAlgorithm = string.IsNullOrWhiteSpace(context.ThumbprintAlgorithm) ? thumbprintAlgorithm : context.ThumbprintAlgorithm;

                if (!string.IsNullOrWhiteSpace(context.CertificateThumbprint) && string.IsNullOrWhiteSpace(context.ThumbprintAlgorithm))
                {
                    context.ThumbprintAlgorithm = ThumbprintAlgorithmStr;
                }

                var existingExtension = extensionList.Find(e => e.Id == availableId);
                if (existingExtension != null)
                {
                    DeleteExtension(availableId);
                }

                AddExtension(new HostedServiceExtensionInput
                {
                    Id = availableId,
                    Thumbprint = context.CertificateThumbprint,
                    ThumbprintAlgorithm = context.ThumbprintAlgorithm,
                    ProviderNameSpace = context.ProviderNameSpace,
                    Type = context.Type,
                    PublicConfiguration = context.PublicConfiguration,
                    PrivateConfiguration = context.PrivateConfiguration
                });

                if (r.RoleType == ExtensionRoleType.NamedRoles)
                {
                    builder.Remove(roleName, context.ProviderNameSpace, context.Type);
                    builder.Add(roleName, availableId);
                }
                else
                {
                    builder.RemoveDefault(context.ProviderNameSpace, context.Type);
                    builder.AddDefault(availableId);
                }
            }

            return builder.ToConfiguration();
        }
    }
}
