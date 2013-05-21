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
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;
    using Model;

    [Cmdlet(VerbsData.Update, "AzureDisk"), OutputType(typeof(DiskContext))]
    public class UpdateAzureDiskCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the disk in the disk library.")]
        [ValidateNotNullOrEmpty]
        public string DiskName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Label of the disk.")]
        [ValidateNotNullOrEmpty]
        public string Label
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            var disk = new Disk
            {
                Name = this.DiskName,
                Label = this.Label
            };

            ExecuteClientActionInOCS(
                disk,
                CommandRuntime.ToString(),
                s => this.Channel.UpdateDisk(s, this.DiskName, disk),
                (op, responseDisk) => new DiskContext
                {
                    DiskName = responseDisk.Name,
                    Label = responseDisk.Label,
                    IsCorrupted = responseDisk.IsCorrupted,
                    AffinityGroup = responseDisk.AffinityGroup,
                    OS = responseDisk.OS,
                    Location = responseDisk.Location,
                    MediaLink = responseDisk.MediaLink,
                    DiskSizeInGB = responseDisk.LogicalDiskSizeInGB,
                    SourceImageName = responseDisk.SourceImageName,
                    AttachedTo = CreateRoleReference(responseDisk.AttachedTo),
                    OperationDescription = CommandRuntime.ToString(),
                    OperationId = op.OperationTrackingId,
                    OperationStatus = op.Status
                });
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }

        private static DiskContext.RoleReference CreateRoleReference(RoleReference roleReference)
        {
            if (roleReference == null)
            {
                return null;
            }

            return new DiskContext.RoleReference
            {
                DeploymentName = roleReference.DeploymentName,
                HostedServiceName = roleReference.HostedServiceName,
                RoleName = roleReference.RoleName
            };
        }
    }
}
