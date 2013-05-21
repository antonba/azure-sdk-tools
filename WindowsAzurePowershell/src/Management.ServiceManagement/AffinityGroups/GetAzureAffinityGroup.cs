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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.AffinityGroups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;


    /// <summary>
    /// List the properties for the specified affinity group.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureAffinityGroup"), OutputType(typeof(AffinityGroupContext))]
    public class GetAzureAffinityGroup : ServiceManagementBaseCmdlet
    {
        public GetAzureAffinityGroup()
        {
        }

        public GetAzureAffinityGroup(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Affinity Group name")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            OnProcessRecord();
        }

        protected override void OnProcessRecord()
        {
            Func<Operation, IEnumerable<AffinityGroup>, object> func = (operation, affinityGroups) =>
                affinityGroups.Select(affinityGroup => new AffinityGroupContext()
                {
                    OperationId = operation.OperationTrackingId,
                    OperationDescription = CommandRuntime.ToString(),
                    OperationStatus = operation.Status,
                    Name = affinityGroup.Name,
                    Label = string.IsNullOrEmpty(affinityGroup.Label) ? null : affinityGroup.Label,
                    Description = affinityGroup.Description,
                    Location = affinityGroup.Location,
                    HostedServices = affinityGroup.HostedServices != null ? affinityGroup.HostedServices.Select(p => new AffinityGroupContext.Service { Url = p.Url, ServiceName = p.ServiceName }) : new AffinityGroupContext.Service[0],
                    StorageServices = affinityGroup.StorageServices != null ? affinityGroup.StorageServices.Select(p => new AffinityGroupContext.Service { Url = p.Url, ServiceName = p.ServiceName }) : new AffinityGroupContext.Service[0]
                });

            if (this.Name != null)
            {
                ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.GetAffinityGroup(s, this.Name), (operation, affinityGroups) => func(operation, new[] { affinityGroups }));
            }
            else
            {
                ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.ListAffinityGroups(s), (operation, affinityGroups) => func(operation, affinityGroups));
            }
        }
    }
}