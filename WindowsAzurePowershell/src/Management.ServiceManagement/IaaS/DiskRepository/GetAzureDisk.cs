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
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;
    using Model;

    [Cmdlet(VerbsCommon.Get, "AzureDisk"), OutputType(typeof(IEnumerable<DiskContext>))]
    public class GetAzureDiskCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = false, HelpMessage = "Name of the disk in the disk library.")]
        [ValidateNotNullOrEmpty]
        public string DiskName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            Func<Operation, IEnumerable<Disk>, object> func = (operation, disks) => disks.Select(d => new DiskContext
            {
                OperationId = operation.OperationTrackingId,
                OperationDescription = CommandRuntime.ToString(),
                OperationStatus = operation.Status,
                DiskName = d.Name,
                Label = d.Label,
                IsCorrupted = d.IsCorrupted,
                AffinityGroup = d.AffinityGroup,
                OS = d.OS,
                Location = d.Location,
                MediaLink = d.MediaLink,
                DiskSizeInGB = d.LogicalDiskSizeInGB,
                SourceImageName = d.SourceImageName,
                AttachedTo = CreateRoleReference(d.AttachedTo)
            }).ToList();
            if (!string.IsNullOrEmpty(this.DiskName))
            {
                ExecuteClientActionInOCS(
                    null,
                    CommandRuntime.ToString(),
                    s => this.Channel.GetDisk(s, this.DiskName),
                    (operation, disk) => func(operation, new[] { disk }));
            }
            else
            {
                ExecuteClientActionInOCS(
                    null,
                    CommandRuntime.ToString(),
                    s => this.Channel.ListDisks(s),
                    (operation, disks) => func(operation, disks));

            }
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