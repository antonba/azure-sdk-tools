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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Walks the specified upgrade domain.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureWalkUpgradeDomain"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureWalkUpgradeDomainCommand : ServiceManagementBaseCmdlet
    {
        public SetAzureWalkUpgradeDomainCommand()
        {
        }

        public SetAzureWalkUpgradeDomainCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot. Staging | Production")]
        [ValidateSet("Staging", "Production", IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, HelpMessage = "Domain number.")]
        [ValidateNotNullOrEmpty]
        public int DomainNumber
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            var walkUpgradeDomain = new WalkUpgradeDomainInput()
            {
                UpgradeDomain = this.DomainNumber
            };

            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.WalkUpgradeDomainBySlot(s, this.ServiceName, this.Slot, walkUpgradeDomain));
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }
    }
}
