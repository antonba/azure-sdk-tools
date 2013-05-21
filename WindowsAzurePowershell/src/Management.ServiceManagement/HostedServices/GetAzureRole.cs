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
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Management.Automation;
    using System.ServiceModel;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;
    using Properties;

    [Cmdlet(VerbsCommon.Get, "AzureRole"), OutputType(typeof(RoleContext))]
    public class GetAzureRoleCommand : ServiceManagementBaseCmdlet
    {
        public GetAzureRoleCommand()
        {
        }

        public GetAzureRoleCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The name of the hosted service.")]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot")]
        [ValidateSet(DeploymentSlotType.Staging, DeploymentSlotType.Production, IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the role.")]
        public string RoleName
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Get Instance Details")]
        public SwitchParameter InstanceDetails
        {
            get;
            set;
        }

        public void GetRoleProcess()
        {
            Operation getDeploymentOperation;
            var currentDeployment = this.GetCurrentDeployment(out getDeploymentOperation);
            if (currentDeployment != null)
            {
                if (this.InstanceDetails.IsPresent == false)
                {
                    var roleContexts = new Collection<RoleContext>();
                    RoleList roles = null;
                    if (string.IsNullOrEmpty(this.RoleName))
                    {
                        roles = currentDeployment.RoleList;
                    }
                    else
                    {
                        roles = new RoleList(currentDeployment.RoleList.Where(r => r.RoleName.Equals(this.RoleName, StringComparison.OrdinalIgnoreCase)));
                    }

                    foreach (var r in roles.Select(role => new RoleContext
                        {
                            InstanceCount = currentDeployment.RoleInstanceList.Count(ri => ri.RoleName.Equals(role.RoleName, StringComparison.OrdinalIgnoreCase)), 
                            RoleName = role.RoleName, 
                            OperationDescription = this.CommandRuntime.ToString(), 
                            OperationStatus = getDeploymentOperation.Status, 
                            OperationId = getDeploymentOperation.OperationTrackingId, 
                            ServiceName = this.ServiceName, 
                            DeploymentID = currentDeployment.PrivateID
                        }))
                    {
                        roleContexts.Add(r);
                    }

                    WriteObject(roleContexts, true);
                }
                else
                {
                    Collection<RoleInstanceContext> instanceContexts = new Collection<RoleInstanceContext>();
                    RoleInstanceList roleInstances = null;

                    if (string.IsNullOrEmpty(this.RoleName))
                    {
                        roleInstances = currentDeployment.RoleInstanceList;
                    }
                    else
                    {
                        roleInstances = new RoleInstanceList(currentDeployment.RoleInstanceList.Where(r => r.RoleName.Equals(this.RoleName, StringComparison.OrdinalIgnoreCase)));
                    }

                    foreach (RoleInstance role in roleInstances)
                    {
                        var context = new RoleInstanceContext()
                        {
                            ServiceName = this.ServiceName,
                            OperationId = getDeploymentOperation.OperationTrackingId,
                            OperationDescription = this.CommandRuntime.ToString(),
                            OperationStatus = getDeploymentOperation.Status,
                            InstanceErrorCode = role.InstanceErrorCode,
                            InstanceFaultDomain = role.InstanceFaultDomain,
                            InstanceName = role.InstanceName,
                            InstanceSize = role.InstanceSize,
                            InstanceStateDetails = role.InstanceStateDetails,
                            InstanceStatus = role.InstanceStatus,
                            InstanceUpgradeDomain = role.InstanceUpgradeDomain,
                            RoleName = role.RoleName,
                            DeploymentID = currentDeployment.PrivateID,
                            InstanceEndpoints = role.InstanceEndpoints
                        };

                        instanceContexts.Add(context);
                    }

                    WriteObject(instanceContexts, true);
                }
            }
        }

        protected override void OnProcessRecord()
        {
            this.GetRoleProcess();
        }

        private Deployment GetCurrentDeployment(out Operation operation)
        {
            using (new OperationContextScope(Channel.ToContextChannel()))
            {
                WriteVerboseWithTimestamp(Resources.GetDeploymentBeginOperation);

                var currentDeployment = this.RetryCall(s => this.Channel.GetDeploymentBySlot(s, this.ServiceName, this.Slot));
                operation = GetOperation();

                WriteVerboseWithTimestamp(Resources.GetDeploymentCompletedOperation);
                return currentDeployment;
            }
        }
    }
}
