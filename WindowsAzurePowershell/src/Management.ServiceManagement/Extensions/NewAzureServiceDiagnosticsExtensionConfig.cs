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
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// New Windows Azure Service Diagnostics Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureServiceDiagnosticsExtensionConfig", DefaultParameterSetName = "NewExtension"), OutputType(typeof(ExtensionConfigurationInput))]
    public class NewAzureServiceDiagnosticsExtensionConfigCommand : BaseAzureServiceDiagnosticsExtensionCmdlet
    {
        public NewAzureServiceDiagnosticsExtensionConfigCommand()
            : base()
        {
        }

        public NewAzureServiceDiagnosticsExtensionConfigCommand(IServiceManagement channel)
            : base(channel)
        {
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [ValidateNotNullOrEmpty]
        public override string[] Role
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "X509Certificate used to encrypt password.")]
        [ValidateNotNullOrEmpty]
        public override X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Thumbprint of a certificate used for encryption.")]
        [ValidateNotNullOrEmpty]
        public override string CertificateThumbprint
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "Algorithm associated with the Thumbprint.")]
        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Algorithm associated with the Thumbprint.")]
        [ValidateNotNullOrEmpty]
        public override string ThumbprintAlgorithm
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "NewExtension", HelpMessage = "Diagnostics Storage Name")]
        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Diagnostics Storage Name")]
        [ValidateNotNullOrEmpty]
        public override string StorageAccountName
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = false, ParameterSetName = "NewExtension", HelpMessage = "Diagnostics Configuration")]
        [Parameter(Position = 4, Mandatory = false, ParameterSetName = "NewExtensionUsingThumbprint", HelpMessage = "Diagnostics Configuration")]
        [ValidateNotNullOrEmpty]
        public XmlDocument DiagnosticsConfiguration
        {
            get;
            set;
        }

        protected override void ValidateParameters()
        {
            ValidateThumbprint(false);
            ValidateStorageAccount();
        }

        public void ExecuteCommand()
        {
            ValidateParameters();
            WriteObject(new ExtensionConfigurationInput
            {
                CertificateThumbprint = CertificateThumbprint,
                ThumbprintAlgorithm = ThumbprintAlgorithm,
                ProviderNameSpace = ExtensionNameSpace,
                Type = ExtensionType,
                PublicConfiguration = string.Format(PublicConfigurationXmlTemplate.ToString(), ConnectionQualifiers, DefaultEndpointsProtocol, StorageAccountName, DiagnosticsConfiguration != null ? DiagnosticsConfiguration.InnerXml : ""),
                PrivateConfiguration = string.Format(PrivateConfigurationXmlTemplate.ToString(), StorageKey),
                X509Certificate = X509Certificate,
                Roles = Role != null && Role.Any() ? Role.Select(r => new ExtensionRole(r)).ToList() : new ExtensionRole[] { new ExtensionRole() }.ToList()
            });
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}
