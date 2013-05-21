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
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;
    using Properties;

    /// <summary>
    /// Requests a reboot/reimage of a single role instance or for all role instances of a role.
    /// </summary>
    [Cmdlet(VerbsCommon.Reset, "AzureRoleInstance", DefaultParameterSetName = "ParameterSetGetDeployment"), OutputType(typeof(ManagementOperationContext))]
    public class ResetAzureRoleInstanceCommand : ServiceManagementBaseCmdlet
    {
        public ResetAzureRoleInstanceCommand()
        {
        }

        public ResetAzureRoleInstanceCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the hosted service.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Slot of the deployment.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(DeploymentSlotType.Staging, DeploymentSlotType.Production, IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the role instance.")]
        [ValidateNotNullOrEmpty]
        public string InstanceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Reboot the role instance.")]
        public SwitchParameter Reboot
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Reimage the role instance.")]
        public SwitchParameter Reimage
        {
            get;
            set;
        }

        public void ExecuteCommand()
        {
            if (InstanceName != null)
            {
                if (Reboot)
                {
                    RebootSingleInstance(InstanceName);
                }
                else if (Reimage)
                {
                    ReimageSingleInstance(InstanceName);
                }
            }
        }

        protected override void OnProcessRecord()
        {
            this.ValidateParameters();
            this.ExecuteCommand();
        }

        private void RebootSingleInstance(string instanceName)
        {
            using (new OperationContextScope(Channel.ToContextChannel()))
            {
                ExecuteClientAction(null, CommandRuntime.ToString(), s => this.Channel.RebootDeploymentRoleInstanceBySlot(s, this.ServiceName, this.Slot, instanceName));
            }
        }

        private void ReimageSingleInstance(string instanceName)
        {
            using (new OperationContextScope(Channel.ToContextChannel()))
            {
                ExecuteClientAction(null, CommandRuntime.ToString(), s => this.Channel.ReimageDeploymentRoleInstanceBySlot(s, this.ServiceName, this.Slot, instanceName));
            }
        }

        private void ValidateParameters()
        {
            if (Reboot && Reimage)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException(Resources.RebootAndReImageAreMutuallyExclusive),
                    string.Empty,
                    ErrorCategory.InvalidData,
                    null));
            }
            else if (!Reboot && !Reimage)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException(Resources.RebootOrReImageAreMissing),
                    string.Empty,
                    ErrorCategory.InvalidData,
                    null));
            }
        }
    }
}
