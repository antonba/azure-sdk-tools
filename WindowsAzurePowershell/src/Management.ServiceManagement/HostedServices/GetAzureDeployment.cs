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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
    using System.Management.Automation;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// View details of a specified deployment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureDeployment"), OutputType(typeof(DeploymentInfoContext))]
    public class GetAzureDeploymentCommand : ServiceManagementBaseCmdlet
    {
        public GetAzureDeploymentCommand()
        {
        }

        public GetAzureDeploymentCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, HelpMessage = "Deployment slot. Staging | Production (default Production)")]
        [ValidateSet(DeploymentSlotType.Staging, DeploymentSlotType.Production, IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        Deployment UpdateDeploymentSlofIfEmpty(Deployment deployment)
        {
            if (string.IsNullOrEmpty(deployment.DeploymentSlot))
            {
                deployment.DeploymentSlot = this.Slot;
            }
            return deployment;
        }

        protected override void OnProcessRecord()
        {
            if (string.IsNullOrEmpty(this.Slot))
            {
                this.Slot = DeploymentSlotType.Production;
            }

            ExecuteClientActionInOCS(
                null,
                CommandRuntime.ToString(),
                s => this.Channel.GetDeploymentBySlot(s, this.ServiceName, this.Slot),
                (operation, deployment) => new DeploymentInfoContext(UpdateDeploymentSlofIfEmpty(deployment))
                {
                    ServiceName = this.ServiceName,
                    OperationId = operation.OperationTrackingId,
                    OperationDescription = CommandRuntime.ToString(),
                    OperationStatus = operation.Status
                });
        }
    }
}