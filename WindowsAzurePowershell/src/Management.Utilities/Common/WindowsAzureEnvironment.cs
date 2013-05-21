// ----------------------------------------------------------------------------------
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
    using Microsoft.WindowsAzure.Management.Utilities.Properties;

    [Serializable]
    public class WindowsAzureEnvironment
    {
        /// <summary>
        /// The Windows Azure environment name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The management portal endpoint.
        /// </summary>
        public string PublishSettingsFileUrl { get; set; }

        /// <summary>
        /// The service management RDFE endpoint.
        /// </summary>
        public string ServiceEndpoint { get; set; }

        /// <summary>
        /// The storage service blob endpoint format.
        /// </summary>
        public string StorageBlobEndpointFormat { get; set; }

        /// <summary>
        /// The storage service queue endpoint format.
        /// </summary>
        public string StorageQueueEndpointFormat { get; set; }

        /// <summary>
        /// The storage service table endpoint format.
        /// </summary>
        public string StorageTableEndpointFormat { get; set; }

        /// <summary>
        /// Gets the endpoint for storage blob.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <returns>The fully qualified uri to the blob service</returns>
        public Uri GetStorageBlobEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageBlobEndpointFormat, useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Gets the endpoint for storage queue.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <returns>The fully qualified uri to the queue service</returns>
        public Uri GetStorageQueueEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageQueueEndpointFormat, useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Gets the endpoint for storage table.
        /// </summary>
        /// <param name="accountName">The account name</param>
        /// <returns>The fully qualified uri to the table service</returns>
        public Uri GetStorageTableEndpoint(string accountName, bool useHttps = true)
        {
            return new Uri(string.Format(StorageTableEndpointFormat, useHttps ? "https" : "http", accountName));
        }

        /// <summary>
        /// Predefined Windows Azure environments
        /// </summary>
        public static Dictionary<string, WindowsAzureEnvironment> PublicEnvironments
        {
            get { return environments; }
            private set { environments = value; }
        }

        private static Dictionary<string, WindowsAzureEnvironment> environments = 
            new Dictionary<string, WindowsAzureEnvironment>(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                EnvironmentName.AzureCloud,
                new WindowsAzureEnvironment()
                {
                    Name = EnvironmentName.AzureCloud,
                    PublishSettingsFileUrl = WindowsAzureEnvironmentConstants.AzurePublishSettingsFileUrl,
                    ServiceEndpoint = WindowsAzureEnvironmentConstants.AzureServiceEndpoint,
                    StorageBlobEndpointFormat = WindowsAzureEnvironmentConstants.AzureStorageBlobEndpointFormat,
                    StorageQueueEndpointFormat = WindowsAzureEnvironmentConstants.AzureStorageQueueEndpointFormat,
                    StorageTableEndpointFormat = WindowsAzureEnvironmentConstants.AzureStorageTableEndpointFormat
                }
            },
            {
                EnvironmentName.AzureChinaCloud,
                new WindowsAzureEnvironment()
                {
                    Name = EnvironmentName.AzureChinaCloud,
                    PublishSettingsFileUrl = WindowsAzureEnvironmentConstants.ChinaPublishSettingsFileUrl,
                    ServiceEndpoint = WindowsAzureEnvironmentConstants.ChinaServiceEndpoint,
                    StorageBlobEndpointFormat = WindowsAzureEnvironmentConstants.ChinaStorageBlobEndpointFormat,
                    StorageQueueEndpointFormat = WindowsAzureEnvironmentConstants.ChinaStorageQueueEndpointFormat,
                    StorageTableEndpointFormat = WindowsAzureEnvironmentConstants.ChinaStorageTableEndpointFormat
                }
            }
        };
    }
}
