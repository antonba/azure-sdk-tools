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
    using System.Management.Automation;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Deletes the specified certificate.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureCertificate"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureCertificate : ServiceManagementBaseCmdlet
    {
        public RemoveAzureCertificate()
        {
        }

        public RemoveAzureCertificate(IServiceManagement channel)
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

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Certificate thumbprint algorithm.")]
        [ValidateNotNullOrEmpty]
        public string ThumbprintAlgorithm
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Certificate thumbprint.")]
        [ValidateNotNullOrEmpty]
        public string Thumbprint
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.DeleteCertificate(s, this.ServiceName, this.ThumbprintAlgorithm, this.Thumbprint));
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }
    }
}
