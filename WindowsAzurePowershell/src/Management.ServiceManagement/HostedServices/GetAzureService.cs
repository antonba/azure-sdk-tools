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
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Retrieve a specified hosted account.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureService"), OutputType(typeof(HostedServiceDetailedContext))]
    public class GetAzureServiceCommand : ServiceManagementBaseCmdlet
    {
        public GetAzureServiceCommand()
        {
        }

        public GetAzureServiceCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            Func<Operation, IEnumerable<HostedService>, object> func = (operation, services) => services.Select(service => new HostedServiceDetailedContext
            {
                ServiceName = service.ServiceName ?? this.ServiceName,
                Url = service.Url,
                Label = string.IsNullOrEmpty(service.HostedServiceProperties.Label)
                            ? string.Empty
                            : service.HostedServiceProperties.Label,
                Description = service.HostedServiceProperties.Description,
                AffinityGroup = service.HostedServiceProperties.AffinityGroup,
                Location = service.HostedServiceProperties.Location,
                Status = service.HostedServiceProperties.Status,
                DateCreated = service.HostedServiceProperties.DateCreated,
                DateModified = service.HostedServiceProperties.DateLastModified,
                OperationId = operation.OperationTrackingId,
                OperationDescription = CommandRuntime.ToString(),
                OperationStatus = operation.Status
            });
            if (this.ServiceName != null)
            {
                ExecuteClientActionInOCS(
                    null,
                    CommandRuntime.ToString(),
                    s => this.Channel.GetHostedService(s, this.ServiceName),
                    (operation, service) => func(operation, new[] { service }));
            }
            else
            {
                ExecuteClientActionInOCS(
                    null,
                    CommandRuntime.ToString(),
                    s => this.Channel.ListHostedServices(s),
                    (operation, service) => func(operation, service));
            }
        }
    }
}
