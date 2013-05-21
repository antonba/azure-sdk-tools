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
    /// Retrieve a Windows Azure Location.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureLocation"), OutputType(typeof(LocationsContext))]
    public class GetAzureLocationCommand : ServiceManagementBaseCmdlet
    {
        public GetAzureLocationCommand()
        {
        }

        public GetAzureLocationCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        public void GetLocationsProcess()
        {

            ExecuteClientActionInOCS(null,
                CommandRuntime.ToString(),
                s => this.Channel.ListLocations(CurrentSubscription.SubscriptionId),
                (op, locations) => locations.Select(location => new LocationsContext
                {
                    OperationId = op.OperationTrackingId,
                    OperationDescription = CommandRuntime.ToString(),
                    OperationStatus = op.Status,
                    DisplayName = location.DisplayName,
                    Name = location.Name,
                    AvailableServices = location.AvailableServices
                }));
        }

        protected override void OnProcessRecord()
        {
            this.GetLocationsProcess();
        }
    }
}
