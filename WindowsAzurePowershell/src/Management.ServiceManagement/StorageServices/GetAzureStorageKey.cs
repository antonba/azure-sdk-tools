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
    using System.Management.Automation;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Displays the primary and secondary keys for the account. Should have 
    /// the storage account resource specified.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureStorageKey"), OutputType(typeof(StorageServiceKeyOperationContext))]
    public class GetAzureStorageKeyCommand : ServiceManagementBaseCmdlet
    {
        public GetAzureStorageKeyCommand()
        {
        }

        public GetAzureStorageKeyCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        [Alias("ServiceName")]
        public string StorageAccountName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            Func<Operation, StorageService, object> func = (operation, storageService) =>
            {
                if (storageService != null)
                {
                    return new StorageServiceKeyOperationContext
                    {
                        StorageAccountName = this.StorageAccountName,
                        Primary = storageService.StorageServiceKeys.Primary,
                        Secondary = storageService.StorageServiceKeys.Secondary,
                        OperationDescription = this.CommandRuntime.ToString(),
                        OperationId = operation.OperationTrackingId,
                        OperationStatus = operation.Status
                    };
                }
                return new StorageServiceKeyOperationContext
                {
                    OperationDescription = this.CommandRuntime.ToString(),
                    OperationId = operation.OperationTrackingId,
                    OperationStatus = operation.Status
                };
            };

            ExecuteClientActionInOCS(
                null,
                CommandRuntime.ToString(),
                s => this.Channel.GetStorageKeys(s, this.StorageAccountName),
                func);
        }
    }
}
