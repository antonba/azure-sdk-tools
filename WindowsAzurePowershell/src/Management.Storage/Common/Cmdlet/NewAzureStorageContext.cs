﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.Storage.Common.Cmdlet
{
    using System;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Microsoft.WindowsAzure.Management.Storage.Model.ResourceModel;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;

    /// <summary>
    /// New storage context
    /// </summary>
    [Cmdlet(VerbsCommon.New, StorageNouns.StorageContext, DefaultParameterSetName = AccountNameKeyParameterSet),
        OutputType(typeof(AzureStorageContext))]
    public class NewAzureStorageContext : CmdletBase
    {
        /// <summary>
        /// Default parameter set name
        /// </summary>
        private const string AccountNameKeyParameterSet = "AccountNameAndKey";

        /// <summary>
        /// Connection string parameter set name
        /// </summary>
        private const string ConnectionStringParameterSet = "ConnectionString";

        /// <summary>
        /// Local development account parameter set name
        /// </summary>
        private const string LocalParameterSet = "LocalDevelopment";

        /// <summary>
        /// Anonymous storage account parameter set name
        /// </summary>
        private const string AnonymousParameterSet = "AnonymousAccount";

        private const string StorageAccountNameHelpMessage = "Azure Storage Acccount Name";
        [Parameter(Position = 0, HelpMessage = StorageAccountNameHelpMessage,
            Mandatory = true, ParameterSetName = AccountNameKeyParameterSet)]
        [Parameter(Position = 0, HelpMessage = StorageAccountNameHelpMessage,
            Mandatory = true, ParameterSetName = AnonymousParameterSet)]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName { get; set; }

        private const string StorageAccountKeyHelpMessage = "Azure Storage Account Key";
        [Parameter(Position = 1, HelpMessage = StorageAccountKeyHelpMessage,
            Mandatory = true, ParameterSetName = AccountNameKeyParameterSet)]
        [ValidateNotNullOrEmpty]
        public string StorageAccountKey { get; set; }

        private const string ConnectionStringHelpMessage = "Azure Storage Connection String";
        [Parameter(HelpMessage = ConnectionStringHelpMessage,
            Mandatory = true, ParameterSetName = ConnectionStringParameterSet)]
        [ValidateNotNullOrEmpty]
        public string ConnectionString { get; set; }

        private const string LocalHelpMessage = "Use local development storage account";
        [Parameter(HelpMessage = LocalHelpMessage,
            Mandatory = true, ParameterSetName = LocalParameterSet)]
        public SwitchParameter Local
        {
            get { return isLocalDevAccount; }
            set { isLocalDevAccount = value; }
        }
        private bool isLocalDevAccount;

        private const string AnonymousHelpMessage = "Use anonymous storage account";
        [Parameter(HelpMessage = AnonymousHelpMessage,
            Mandatory = true, ParameterSetName = AnonymousParameterSet)]
        public SwitchParameter Anonymous
        {
            get { return isAnonymous; }
            set { isAnonymous = value; }
        }
        private bool isAnonymous;

        private const string ProtocolHelpMessage = "Protocol specification (HTTP or HTTPS), default is HTTPS";
        [Parameter(HelpMessage = ProtocolHelpMessage,
            ParameterSetName = AccountNameKeyParameterSet)]
        [Parameter(HelpMessage = ProtocolHelpMessage,
            ParameterSetName = AnonymousParameterSet)]
        [ValidateSet(StorageNouns.HTTP, StorageNouns.HTTPS, IgnoreCase = true)]
        public string Protocol
        {
            get { return protocolType; }
            set { protocolType = value; }
        }
        private string protocolType = StorageNouns.HTTPS;

        private  const string EndPointHelpMessage = "Azure storage endpoint";
        [Parameter(HelpMessage = EndPointHelpMessage, ParameterSetName = AccountNameKeyParameterSet)]
        [Parameter(HelpMessage = EndPointHelpMessage, ParameterSetName = AnonymousParameterSet)]
        public string EndPoint
        {
            get { return storageEndPoint; }
            set { storageEndPoint = value; }
        }
        private string storageEndPoint = string.Empty;

        /// <summary>
        /// Get storage account by account name and account key
        /// </summary>
        /// <param name="accountName">Storage account name</param>
        /// <param name="accountKey">Storage account key</param>
        /// <param name="useHttps">Use https or not</param>
        /// <returns>A storage account</returns>
        internal CloudStorageAccount GetStorageAccountByNameAndKey(string accountName, string accountKey,
            bool useHttps, string storageEndPoint = "")
        {
            StorageCredentials credential = new StorageCredentials(accountName, accountKey);
            return GetStorageAccountWithEndPoint(credential, accountName, useHttps, storageEndPoint);
        }

        /// <summary>
        /// Get storage account by sastoken
        /// </summary>
        /// <param name="storageAccountName">Storage account name, it's used for build end point</param>
        /// <param name="sasToken">Sas token</param>
        /// <param name="useHttps">Use https or not</param>
        /// <returns>a storage account</returns>
        internal CloudStorageAccount GetStorageAccountBySasToken(string storageAccountName, string sasToken, bool useHttps)
        {
            StorageCredentials credential = new StorageCredentials(sasToken);
            return GetStorageAccountWithEndPoint(credential, storageAccountName, useHttps);
        }

        /// <summary>
        /// Get storage account by connection string
        /// </summary>
        /// <param name="connectionString">Azure storage connection string</param>
        /// <returns>A storage account</returns>
        internal CloudStorageAccount GetStorageAccountByConnectionString(string connectionString)
        {
            return CloudStorageAccount.Parse(connectionString);
        }

        /// <summary>
        /// Get local development storage account
        /// </summary>
        /// <returns>A storage account</returns>
        internal CloudStorageAccount GetLocalDevelopmentStorageAccount()
        {
            return CloudStorageAccount.DevelopmentStorageAccount;
        }

        /// <summary>
        /// Get anonymous storage account
        /// </summary>
        /// <param name="storageAccountName">Storage account name, it's used for build end point</param>
        /// <returns>A storage account</returns>
        internal CloudStorageAccount GetAnonymousStorageAccount(string storageAccountName, bool useHttps, string storageEndPoint = "")
        {
            StorageCredentials credential = new StorageCredentials();
            return GetStorageAccountWithEndPoint(credential, storageAccountName, useHttps, storageEndPoint);
        }

        /// <summary>
        /// Get storage account and use specific end point
        /// </summary>
        /// <param name="credential">Storage credentail</param>
        /// <param name="storageAccountName">Storage account name, it's used for build end point</param>
        /// <returns>A storage account</returns>
        internal CloudStorageAccount GetStorageAccountWithEndPoint(StorageCredentials credential,
            string storageAccountName, bool useHttps, string endPoint = "")
        {
            if (String.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentException(String.Format(Resources.ObjectCannotBeNull, StorageNouns.StorageAccountName));
            }

            string blobEndPoint = string.Empty;
            string tableEndPoint = string.Empty;
            string queueEndPoint = string.Empty;
            string domain = string.Empty;

            if (string.IsNullOrEmpty(endPoint))
            {
                domain = GetDefaultEndPointDomain();
            }
            else
            {
                domain = GetStorageDomainFromEndPoint(endPoint);
            }
            
            if (useHttps)
            {
                blobEndPoint = String.Format(Resources.HttpsBlobEndPointFormat, storageAccountName, domain);
                tableEndPoint = String.Format(Resources.HttpsTableEndPointFormat, storageAccountName, domain);
                queueEndPoint = String.Format(Resources.HttpsQueueEndPointFormat, storageAccountName, domain);
            }
            else
            {
                blobEndPoint = String.Format(Resources.HttpBlobEndPointFormat, storageAccountName, domain);
                tableEndPoint = String.Format(Resources.HttpTableEndPointFormat, storageAccountName, domain);
                queueEndPoint = String.Format(Resources.HttpQueueEndPointFormat, storageAccountName, domain);
            }

            return new CloudStorageAccount(credential, new Uri(blobEndPoint), new Uri(queueEndPoint), new Uri(tableEndPoint));
        }

        /// <summary>
        /// Get default end point domain
        /// </summary>
        /// <returns></returns>
        internal string GetDefaultEndPointDomain()
        {
            return Resources.DefaultStorageEndPointDomain;
        }

        /// <summary>
        /// Get endpoint domain from endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        internal string GetStorageDomainFromEndPoint(string endpoint)
        {
            string domain = string.Empty;
            Uri uri = SafeGetUri(endpoint);

            string endPointSignature = "core."; //blob.core.windows.net
            int index = uri.Authority.ToLower().LastIndexOf(endPointSignature);

            if (index != -1)
            {
                domain = uri.Authority.Substring(index + endPointSignature.Length).ToLower();
            }

            if(string.IsNullOrEmpty(domain) || uri.IsFile)
            {
                throw new ArgumentException(string.Format(Resources.InvalidStorageEndPoint, endpoint), "EndPoint");
            }

            return domain;
        }

        /// <summary>
        /// safely get uri object from end point
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns>Uri object</returns>
        private Uri SafeGetUri(string endpoint)
        {
            Uri uri = null;

            try
            {
                endpoint = endpoint.Trim();
                bool created = Uri.TryCreate(endpoint, UriKind.Absolute, out uri);

                if (!created || uri.HostNameType == UriHostNameType.Unknown)
                {
                    uri = new Uri(Resources.HTTPPrefix + endpoint);
                }
            }
            catch
            {
                throw new ArgumentException(string.Format(Resources.InvalidStorageEndPoint, endpoint), "EndPoint");
            }

            return uri;
        }

        /// <summary>
        /// Execute command
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            CloudStorageAccount account = null;
            bool useHttps = (StorageNouns.HTTPS.ToLower() == protocolType.ToLower());

            switch (ParameterSetName)
            {
                case AccountNameKeyParameterSet:
                    account = GetStorageAccountByNameAndKey(StorageAccountName, StorageAccountKey, useHttps, storageEndPoint);
                    break;
                case ConnectionStringParameterSet:
                    account = GetStorageAccountByConnectionString(ConnectionString);
                    break;
                case LocalParameterSet:
                    account = GetLocalDevelopmentStorageAccount();
                    break;
                case AnonymousParameterSet:
                    account = GetAnonymousStorageAccount(StorageAccountName, useHttps, storageEndPoint);
                    break;
                default:
                    throw new ArgumentException(Resources.DefaultStorageCredentialsNotFound);
            }

            AzureStorageContext context = new AzureStorageContext(account);
            WriteObject(context);
        }
    }
}
