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
    using System.Management.Automation;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Deletes an affinity group.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureAffinityGroup"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureAffinityGroup : ServiceManagementBaseCmdlet
    {
        public RemoveAzureAffinityGroup()
        {
        }

        public RemoveAzureAffinityGroup(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true, HelpMessage = "Affinity Group name.")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            this.ExecuteClientActionInOCS(null, this.CommandRuntime.ToString(), s => this.Channel.DeleteAffinityGroup(s, this.Name));
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}
