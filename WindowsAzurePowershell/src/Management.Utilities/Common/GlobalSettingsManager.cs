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

namespace Microsoft.WindowsAzure.Management.Utilities.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Web.Script.Serialization;
    using Microsoft.WindowsAzure.Management.Utilities.Common.XmlSchema;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using Microsoft.WindowsAzure.Management.Utilities.Subscription;

    public class GlobalSettingsManager
    {
        private Dictionary<string, WindowsAzureEnvironment> environments;

        public GlobalPathInfo GlobalPaths { get; private set; }

        public PublishData PublishSettings { get; private set; }

        public X509Certificate2 Certificate { get; private set; }

        public SubscriptionsManager SubscriptionManager { get; private set; }

        public CloudServiceProjectConfiguration ServiceConfiguration { get; private set; }

        public IDictionary<string, SubscriptionData> Subscriptions
        {
            get { return SubscriptionManager.Subscriptions; }
        }

        public WindowsAzureEnvironment DefaultEnvironment { get; set; }

        protected GlobalSettingsManager(string azurePath)
            : this(azurePath, null)
        {
        }

        protected GlobalSettingsManager(string azurePath, string subscriptionsDataFile)
        {
            GlobalPaths = new GlobalPathInfo(azurePath, subscriptionsDataFile);
            DefaultEnvironment = WindowsAzureEnvironment.PublicEnvironments[EnvironmentName.AzureCloud];
            environments = new Dictionary<string, WindowsAzureEnvironment>(
                WindowsAzureEnvironment.PublicEnvironments,
                StringComparer.InvariantCultureIgnoreCase);
        }

        public static GlobalSettingsManager CreateFromPublishSettings(
            string azurePath,
            string subscriptionsDataFile,
            string publishSettingsFile)
        {
            Validate.ValidateNullArgument(azurePath, string.Format(Resources.InvalidNullArgument, "azurePath"));

            var globalSettingsManager = new GlobalSettingsManager(azurePath, subscriptionsDataFile);
            globalSettingsManager.NewFromPublishSettings(
                globalSettingsManager.GlobalPaths.SubscriptionsDataFile,
                publishSettingsFile);
            globalSettingsManager.Save();

            return globalSettingsManager;
        }

        public static GlobalSettingsManager Create(
            string azurePath,
            string subscriptionsDataFile,
            X509Certificate2 certificate,
            string serviceEndpoint)
        {
            Validate.ValidateNullArgument(azurePath, string.Format(Resources.InvalidNullArgument, "azurePath"));
            Validate.ValidateNullArgument(certificate, string.Format(Resources.InvalidCertificateSingle, "certificate"));
            Validate.ValidateNullArgument(serviceEndpoint, string.Format(Resources.InvalidEndpoint, "serviceEndpoint"));

            var globalSettingsManager = new GlobalSettingsManager(azurePath, subscriptionsDataFile);
            globalSettingsManager.New(globalSettingsManager.GlobalPaths.SubscriptionsDataFile, certificate, serviceEndpoint);
            globalSettingsManager.Save();

            return globalSettingsManager;
        }

        public static GlobalSettingsManager Load(string azurePath)
        {
            return Load(azurePath, null);
        }

        public static GlobalSettingsManager Load(string azurePath, string subscriptionsDataFile)
        {
            Validate.ValidateNullArgument(azurePath, string.Format(Resources.InvalidNullArgument, "azurePath"));
            Validate.ValidateStringIsNullOrEmpty(azurePath, Resources.AzureDirectoryName);

            var globalSettingsManager = new GlobalSettingsManager(azurePath, subscriptionsDataFile);
            globalSettingsManager.LoadCurrent();

            return globalSettingsManager;
        }

        private void NewFromPublishSettings(string subscriptionsDataFile, string publishSettingsPath)
        {
            Validate.ValidateStringIsNullOrEmpty(GlobalPaths.AzureDirectory, Resources.AzureDirectoryName);
            Validate.ValidateFileFull(publishSettingsPath, Resources.PublishSettings);
            Validate.ValidateFileExtention(publishSettingsPath, Resources.PublishSettingsFileExtention);

            PublishSettings = General.DeserializeXmlFile<PublishData>(publishSettingsPath, string.Format(Resources.InvalidPublishSettingsSchema, publishSettingsPath));
            if (!string.IsNullOrEmpty(PublishSettings.Items.First().ManagementCertificate))
            {
                Certificate = new X509Certificate2(Convert.FromBase64String(PublishSettings.Items.First().ManagementCertificate), string.Empty);
                PublishSettings.Items.First().ManagementCertificate = Certificate.Thumbprint;
            }
            
            SubscriptionManager = SubscriptionsManager.Import(subscriptionsDataFile, PublishSettings, Certificate);
            ServiceConfiguration = new CloudServiceProjectConfiguration
            {
                endpoint = PublishSettings.Items.First().Url,
                subscription = PublishSettings.Items.First().Subscription.First().Id,
                subscriptionName = PublishSettings.Items.First().Subscription.First().Name
            };
        }

        private void New(string subscriptionsDataFile, X509Certificate2 certificate, string serviceEndpoint)
        {
            Validate.ValidateStringIsNullOrEmpty(GlobalPaths.AzureDirectory, Resources.AzureDirectoryName);

            Certificate = certificate;
            SubscriptionManager = SubscriptionsManager.Import(subscriptionsDataFile, null, certificate);
            ServiceConfiguration = new CloudServiceProjectConfiguration { endpoint = serviceEndpoint };
            PublishSettings = new PublishData();

            var publishDataProfile = new PublishDataPublishProfile
            {
                Url = ServiceConfiguration.endpoint
            };

            if (Certificate != null)
            {
                publishDataProfile.ManagementCertificate = certificate.Thumbprint;
            }

            if (SubscriptionManager.Subscriptions != null &&
                SubscriptionManager.Subscriptions.Count > 0)
            {
                var subscription = SubscriptionManager.Subscriptions.Values.First();

                ServiceConfiguration.subscription = subscription.SubscriptionId;
                ServiceConfiguration.subscriptionName = subscription.SubscriptionName;
                publishDataProfile.Subscription = new [] {
                    new PublishDataPublishProfileSubscription
                    {
                        Id = subscription.SubscriptionId,
                        Name = subscription.SubscriptionName
                    }
                };
            }

            PublishSettings.Items = new [] { publishDataProfile };
        }

        internal void LoadCurrent()
        {
            Validate.ValidateDirectoryExists(GlobalPaths.AzureDirectory, Resources.GlobalSettingsManager_Load_PublishSettingsNotFound);
            Validate.ValidateFileExists(GlobalPaths.PublishSettingsFile, string.Format(Resources.PathDoesNotExistForElement, Resources.PublishSettingsFileName, GlobalPaths.PublishSettingsFile));

            PublishSettings = General.DeserializeXmlFile<PublishData>(GlobalPaths.PublishSettingsFile);
            if (!string.IsNullOrEmpty(PublishSettings.Items.First().ManagementCertificate))
            {
                Certificate = General.GetCertificateFromStore(PublishSettings.Items.First().ManagementCertificate);
            }

            SubscriptionManager = SubscriptionsManager.Import(GlobalPaths.SubscriptionsDataFile);
            ServiceConfiguration = new JavaScriptSerializer().Deserialize<CloudServiceProjectConfiguration>(File.ReadAllText(GlobalPaths.ServiceConfigurationFile));
            var defaultSubscription = SubscriptionManager.Subscriptions.Values.FirstOrDefault(subscription => 
                subscription.SubscriptionId == ServiceConfiguration.subscription &&
                (string.IsNullOrEmpty(ServiceConfiguration.subscriptionName) || subscription.SubscriptionName == ServiceConfiguration.subscriptionName));
            if (defaultSubscription != null)
            {
                defaultSubscription.IsDefault = true;
            }
        }

        internal void Save()
        {
            // Create new Azure directory if doesn't exist
            //
            Directory.CreateDirectory(GlobalPaths.AzureDirectory);

            // Save *.publishsettings
            //
            General.SerializeXmlFile(PublishSettings, GlobalPaths.PublishSettingsFile);

            // Save certificate in the store
            //
            if (Certificate != null)
            {
                General.AddCertificateToStore(Certificate);
            }

            // Save service configuration
            //
            File.WriteAllText(GlobalPaths.ServiceConfigurationFile, new JavaScriptSerializer().Serialize(ServiceConfiguration));

            // Save subscriptions
            //
            SubscriptionManager.SaveSubscriptions(GlobalPaths.SubscriptionsDataFile);
        }

        internal void SaveSubscriptions()
        {
            SaveSubscriptions(null);
        }

        internal void SaveSubscriptions(string subscriptionDataFile)
        {
            if (subscriptionDataFile == null)
            {
                subscriptionDataFile = GlobalPaths.SubscriptionsDataFile;
            }

            SubscriptionManager.SaveSubscriptions(subscriptionDataFile);

            var defaultSubscription = SubscriptionManager.Subscriptions.Values.FirstOrDefault(s => s.IsDefault);
            if (defaultSubscription != null)
            {
                ServiceConfiguration.subscription = defaultSubscription.SubscriptionId;
                ServiceConfiguration.subscriptionName = defaultSubscription.SubscriptionName;
                ServiceConfiguration.endpoint = defaultSubscription.ServiceEndpoint;
                File.WriteAllText(GlobalPaths.ServiceConfigurationFile, new JavaScriptSerializer().Serialize(ServiceConfiguration));
            }
        }

        public string GetSubscriptionId(string subscriptionName)
        {
            foreach (var subscription in Subscriptions.Values)
            {
                if (subscription.SubscriptionName.Equals(subscriptionName))
                {
                    Validate.IsGuid(subscription.SubscriptionId);
                    return subscription.SubscriptionId;
                }
            }

            throw new ArgumentException(string.Format(Resources.SubscriptionIdNotFoundMessage, subscriptionName, GlobalPaths.PublishSettingsFile));
        }

        internal void DeleteGlobalSettingsManager()
        {
            General.RemoveCertificateFromStore(Certificate);
            File.Delete(GlobalPaths.PublishSettingsFile);
            File.Delete(GlobalPaths.SubscriptionsDataFile);
            File.Delete(GlobalPaths.ServiceConfigurationFile);
            Directory.Delete(GlobalPaths.AzureDirectory, true);
        }

        /// <summary>
        /// Gets the current instance of GlobalSettingsManager using GlobalPathInfo.AzureAppDir.
        /// </summary>
        public static GlobalSettingsManager Instance { get { return Load(GlobalPathInfo.AzureAppDir); } }

        /// <summary>
        /// Gets url for downloading publish settings file.
        /// </summary>
        /// <param name="environment">The environment name</param>
        /// <param name="realm">The optional realm phrase</param>
        /// <returns>The publish settings file url</returns>
        public string GetPublishSettingsFile(string environment = null, string realm = null)
        {
            // If no environment provided assume using the current environment.
            environment = string.IsNullOrEmpty(environment) ? DefaultEnvironment.Name : environment;
            WindowsAzureEnvironment environmentObject = GetEnvironment(environment);
            Debug.Assert(!string.IsNullOrEmpty(environmentObject.PublishSettingsFileUrl));

            StringBuilder publishSettingsUrl = new StringBuilder(environmentObject.PublishSettingsFileUrl);

            if (!string.IsNullOrEmpty(realm))
            {
                publishSettingsUrl.AppendFormat(Resources.RealmFormat, realm);
            }

            return publishSettingsUrl.ToString();
        }

        /// <summary>
        /// Gets the environment instance by name.
        /// </summary>
        /// <param name="environmentName">The environment name</param>
        /// <returns>The environment instance</returns>
        public WindowsAzureEnvironment GetEnvironment(string environmentName)
        {
            WindowsAzureEnvironment environment;
            if (!environments.TryGetValue(environmentName, out environment))
            {
                throw new KeyNotFoundException(string.Format(Resources.EnvironmentNotFound, environmentName));
            }

            return environment;
        }

        /// <summary>
        /// Gets list if all available Windows Azure environments.
        /// </summary>
        /// <returns>The windows azure environments list</returns>
        public List<WindowsAzureEnvironment> GetEnvironments()
        {
            return environments.Values.ToList();
        }
    }
}