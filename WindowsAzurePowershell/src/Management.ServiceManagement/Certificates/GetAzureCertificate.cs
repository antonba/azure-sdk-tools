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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Certificates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;
    using Properties;


    /// <summary>
    /// Retrieve a specified service certificate.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureCertificate"), OutputType(typeof(CertificateContext))]
    public class GetAzureCertificate : ServiceManagementBaseCmdlet
    {
        public GetAzureCertificate()
        {
        }

        public GetAzureCertificate(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Hosted Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Certificate thumbprint algorithm.")]
        [ValidateNotNullOrEmpty]
        public string ThumbprintAlgorithm
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Certificate thumbprint.")]
        [ValidateNotNullOrEmpty]
        public string Thumbprint
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            Func<Operation, IEnumerable<Certificate>, object> func = (operation, certificates) => certificates.Select(certificate => new CertificateContext
            {
                ServiceName = this.ServiceName,
                Data = certificate.Data,
                Thumbprint = certificate.Thumbprint,
                ThumbprintAlgorithm = certificate.ThumbprintAlgorithm,
                Url = certificate.CertificateUrl,
                OperationId = operation.OperationTrackingId,
                OperationDescription = CommandRuntime.ToString(),
                OperationStatus = operation.Status
            });
            if (this.Thumbprint != null)
            {
                if (this.ThumbprintAlgorithm == null)
                {
                    throw new ArgumentNullException("ThumbprintAlgorithm", Resources.MissingThumbprintAlgorithm);
                }
                ExecuteClientActionInOCS(
                    null,
                    CommandRuntime.ToString(),
                    s => this.Channel.GetCertificate(s, this.ServiceName, this.ThumbprintAlgorithm, this.Thumbprint),
                    (operation, certificate) => func(operation, new[] { certificate }));
            }
            else
            {
                ExecuteClientActionInOCS(
                     null,
                     CommandRuntime.ToString(),
                     s => this.Channel.ListCertificates(s, this.ServiceName),
                     (operation, certificates) => func(operation, certificates));
            }
        }

        public void ExecuteCommand()
        {
            OnProcessRecord();
        }
    }
}