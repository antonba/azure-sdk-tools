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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Helpers
{
    using System;
    using System.ServiceModel;
    using Storage.Auth;
    using Storage.Blob;
    using WindowsAzure.ServiceManagement;

    public static class Disks
    {
        public static void RemoveVHD(IServiceManagement channel, string subscriptionId, Uri mediaLink)
        {            
            var accountName = mediaLink.Host.Split('.')[0];
            var blobEndpoint = new Uri(mediaLink.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped));

            StorageService storageService;
            using (new OperationContextScope(channel.ToContextChannel()))
            {
                storageService = channel.GetStorageKeys(subscriptionId, accountName);
            }

            var storageAccountCredentials = new StorageCredentials(accountName, storageService.StorageServiceKeys.Primary);
            var client = new CloudBlobClient(blobEndpoint, storageAccountCredentials);
            var blob = client.GetBlobReferenceFromServer(mediaLink);
            blob.DeleteIfExists();
        }
    }
}