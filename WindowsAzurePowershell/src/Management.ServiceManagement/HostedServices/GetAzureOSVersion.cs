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
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Lists the versions of the guest operating system that are currently available in Windows Azure.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureOSVersion"), OutputType(typeof(OSVersionsContext))]
    public class GetAzureOSVersionCommand : ServiceManagementBaseCmdlet
    {
        public GetAzureOSVersionCommand()
        {
        }

        public GetAzureOSVersionCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        protected override void OnProcessRecord()
        {
            ExecuteClientActionInOCS(
                null,
                CommandRuntime.ToString(),
                s => this.Channel.ListOperatingSystems(s),
                (operation, operatingSystems) => operatingSystems.Select(os => new OSVersionsContext
                {
                    OperationId = operation.OperationTrackingId,
                    OperationDescription = CommandRuntime.ToString(),
                    OperationStatus = operation.Status,
                    Family = os.Family,
                    FamilyLabel = string.IsNullOrEmpty(os.FamilyLabel) ? null : os.FamilyLabel,
                    IsActive = os.IsActive,
                    IsDefault = os.IsDefault,
                    Version = os.Version,
                    Label = string.IsNullOrEmpty(os.Label) ? null : os.Label
                })
                );
        }
    }
}