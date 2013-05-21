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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
    using System.Management.Automation;
    using WindowsAzure.ServiceManagement;
    using Utilities.Common;
    using Model;


    [Cmdlet(VerbsLifecycle.Start, "AzureVM", DefaultParameterSetName = "ByName"), OutputType(typeof(ManagementOperationContext))]
    public class StartAzureVMCommand : IaaSDeploymentManagementCmdletBase
    {
        public StartAzureVMCommand()
        {
        }

        public StartAzureVMCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the Virtual Machine to start.", ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Virtual Machine to restart.", ParameterSetName = "Input")]
        [ValidateNotNullOrEmpty]
        [Alias("InputObject")]
        public PersistentVM VM
        {
            get;
            set;
        }

        internal override void ExecuteCommand()
        {
            base.ExecuteCommand();

            if (CurrentDeployment == null)
            {
                return;
            }

            string roleName = (this.ParameterSetName == "ByName") ? this.Name : this.VM.RoleName;
            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.StartRole(s, this.ServiceName, CurrentDeployment.Name, roleName));
        }

    }
}
