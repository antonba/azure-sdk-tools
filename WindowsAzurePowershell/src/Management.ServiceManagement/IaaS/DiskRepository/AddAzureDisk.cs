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
    using System;
    using System.Management.Automation;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;


    [Cmdlet(VerbsCommon.Add, "AzureDisk"), OutputType(typeof(DiskContext))]
    public class AddAzureDiskCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the disk in the disk library.")]
        [ValidateNotNullOrEmpty]
        public string DiskName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Location of the physical blob backing the disk. This link refers to a blob in a storage account.")]
        [ValidateNotNullOrEmpty]
        public string MediaLocation
        {
            get;
            set;
        }

        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Label of the disk.")]
        [ValidateNotNullOrEmpty]
        public string Label
        {
            get;
            set;
        }

        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "OS on Disk.")]
        [ValidateNotNullOrEmpty]
        public string OS
        {
            get;
            set;
        }

        public void ExecuteCommand()
        {
            var disk = new Disk
            {
                Name = this.DiskName,
                MediaLink = new Uri(this.MediaLocation),
                OS = this.OS,
                Label = string.IsNullOrEmpty(this.Label) ? this.DiskName : this.Label
            };

            ExecuteClientActionInOCS(
                disk, 
                CommandRuntime.ToString(), 
                s => this.Channel.CreateDisk(s, disk), 
                (op,responseDisk) => new DiskContext
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
