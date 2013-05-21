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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.StorageServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Lists all storage services underneath the subscription.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureStorageAccount"), OutputType(typeof(StorageServicePropertiesOperationContext))]
    public class GetAzureStorageAccountCommand : ServiceManagementBaseCmdlet
    {
        public GetAzureStorageAccountCommand()
        {
        }

        public GetAzureStorageAccountCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Storage Account Name.")]
        [ValidateNotNullOrEmpty]
        public string StorageAccountName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            Func<Operation, IEnumerable<StorageService>, object> func = (operation, storageServices) => storageServices.Select(service => new StorageServicePropertiesOperationContext()
            {
                StorageAccountName = service.ServiceName,
                OperationId = operation.OperationTrackingId,
                OperationDescription = CommandRuntime.ToString(),
                OperationStatus = operation.Status,
                AffinityGroup = service.StorageServiceProperties.AffinityGroup,
                StorageAccountDescription = service.StorageServiceProperties.Description,
                Label = String.IsNullOrEmpty(service.StorageServiceProperties.Label) ? string.Empty : service.StorageServiceProperties.Label,
                Location = service.StorageServiceProperties.Location,
                Endpoints = service.StorageServiceProperties.Endpoints,
                StorageAccountStatus = service.StorageServiceProperties.Status,
                GeoReplicationEnabled = service.StorageServiceProperties.GeoReplicationEnabled,
                GeoPrimaryLocation = service.StorageServiceProperties.GeoPrimaryRegion,
                GeoSecondaryLocation = service.StorageServiceProperties.StatusOfSecondary,
                StatusOfPrimary = service.StorageServiceProperties.StatusOfPrimary,
                StatusOfSecondary = service.StorageServiceProperties.StatusOfSecondary
            });


            if (!string.IsNullOrEmpty(this.StorageAccountName))
            {
                ExecuteClientActionInOCS(
                    null,
                    CommandRuntime.ToString(),
                    s => this.Channel.GetStorageService(s, this.StorageAccountName),
                    (operation, storageService) => func(operation, new[] { storageService }));
            }
            else
            {
                ExecuteClientActionInOCS(
                    null,
                    CommandRuntime.ToString(),
                    s => this.Channel.ListStorageServices(s),
                    (operation, storageServices) => func(operation, storageServices));
            }
        }
    }
}
