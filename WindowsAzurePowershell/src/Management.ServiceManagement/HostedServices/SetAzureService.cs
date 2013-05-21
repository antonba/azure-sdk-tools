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
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;
    using Properties;

    /// <summary>
    /// Sets the label and description of the specified hosted service
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureService"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureServiceCommand : ServiceManagementBaseCmdlet
    {
        public SetAzureServiceCommand()
        {
        }

        public SetAzureServiceCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "A label for the hosted service. The label may be up to 100 characters in length.")]
        [ValidateLength(0, 100)]
        public string Label
        {
            get;
            set;
        }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true, HelpMessage = "A description for the hosted service. The description may be up to 1024 characters in length.")]
        [ValidateLength(0, 1024)]
        public string Description
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            if (this.Label == null && this.Description == null)
            {
                ThrowTerminatingError(new ErrorRecord(
                                               new Exception(
                                               Resources.LabelOrDescriptionMustBeSpecified),
                                               string.Empty,
                                               ErrorCategory.InvalidData,
                                               null));
            }

            var updateHostedServiceInput = new UpdateHostedServiceInput
            {
                Label = this.Label ?? null,
                Description =  this.Description
            };

            ExecuteClientActionInOCS(updateHostedServiceInput, CommandRuntime.ToString(), s => this.Channel.UpdateHostedService(s, this.ServiceName, updateHostedServiceInput));
        }
    }
}
