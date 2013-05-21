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
    using System.Text;
    using System.Xml.Linq;
    using Model;
    using Properties;
    using WindowsAzure.ServiceManagement;

    public abstract class BaseAzureServiceDiagnosticsExtensionCmdlet : BaseAzureServiceExtensionCmdlet
    {
        protected const string ConnectionQualifiersElemStr = "ConnectionQualifiers";
        protected const string DefaultEndpointsProtocolElemStr = "DefaultEndpointsProtocol";
        protected const string StorageAccountElemStr = "StorageAccount";
        protected const string StorageNameElemStr = "Name";
        protected const string StorageKeyElemStr = "StorageKey";
        protected const string WadCfgElemStr = "WadCfg";
        protected const string DiagnosticsExtensionNamespace = "Microsoft.Windows.Azure.Extensions";
        protected const string DiagnosticsExtensionType = "Diagnostics";

        protected string StorageKey;
        protected string ConnectionQualifiers;
        protected string DefaultEndpointsProtocol;

        public virtual string StorageAccountName
        {
            get;
            set;
        }

        public BaseAzureServiceDiagnosticsExtensionCmdlet()
            : base()
        {
            Initialize();
        }

        public BaseAzureServiceDiagnosticsExtensionCmdlet(IServiceManagement channel)
            : base(channel)
        {
            Initialize();
        }

        protected void ValidateStorageAccount()
        {
            var storageService = Channel.GetStorageService(CurrentSubscription.SubscriptionId, StorageAccountName);
            if (storageService == null)
            {
                throw new Exception(string.Format(Resources.ServiceExtensionCannotFindStorageAccountName, StorageAccountName));
            }

            StringBuilder endpointStr = new StringBuilder();
            endpointStr.AppendFormat("BlobEndpoint={0}", storageService.StorageServiceProperties.Endpoints[0]);
            endpointStr.AppendFormat(";QueueEndpoint={0}", storageService.StorageServiceProperties.Endpoints[1]);
            endpointStr.AppendFormat(";TableEndpoint={0}", storageService.StorageServiceProperties.Endpoints[2]);
            endpointStr.Replace("http://", "https://");

            DefaultEndpointsProtocol = "https";
            ConnectionQualifiers = endpointStr.ToString();

            var storageKeys = Channel.GetStorageKeys(CurrentSubscription.SubscriptionId, StorageAccountName);
            if (storageKeys == null)
            {
                throw new Exception(string.Format(Resources.ServiceExtensionCannotFindStorageAccountKey, StorageAccountName));
            }
            StorageKey = storageKeys.StorageServiceKeys.Primary;
        }

        protected void Initialize()
        {
            ExtensionNameSpace = DiagnosticsExtensionNamespace;
            ExtensionType = DiagnosticsExtensionType;

            XNamespace configNameSpace = "http://schemas.microsoft.com/ServiceHosting/2010/10/DiagnosticsConfiguration";

            PublicConfigurationXmlTemplate = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(configNameSpace + PublicConfigStr,
                    new XElement(configNameSpace + StorageAccountElemStr,
                        new XElement(configNameSpace + ConnectionQualifiersElemStr, "{0}"),
                        new XElement(configNameSpace + DefaultEndpointsProtocolElemStr, "{1}"),
                        new XElement(configNameSpace + StorageNameElemStr, "{2}")
                    ),
                    new XElement(configNameSpace + WadCfgElemStr, "{3}")
                )
            );

            PrivateConfigurationXmlTemplate = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XProcessingInstruction("xml-stylesheet", @"type=""text/xsl"" href=""style.xsl"""),
                new XElement(PrivateConfigStr,
                    new XElement(StorageKeyElemStr, "{0}")
                )
            );
        }
    }
}
