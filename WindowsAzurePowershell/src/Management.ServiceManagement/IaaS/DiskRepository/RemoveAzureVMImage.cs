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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.DiskRepository
{
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using Helpers;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    [Cmdlet(VerbsCommon.Remove, "AzureVMImage"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureVMImage : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the image in the image library to remove.")]
        [ValidateNotNullOrEmpty]
        public string ImageName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, HelpMessage = "Specify to remove the underlying VHD from the blob storage.")]
        public SwitchParameter DeleteVHD
        {
            get;
            set;
        }

        public void RemoveVMImageProcess()
        {
            try
            {
                Uri mediaLink = null;
                if (this.DeleteVHD.IsPresent)
                {
                    // Get the location of the underlying VHD
                    using (new OperationContextScope(Channel.ToContextChannel()))
                    {
                        var image = this.RetryCall(s => this.Channel.GetOSImage(s, this.ImageName));
                        mediaLink = image.MediaLink;
                    }
                }

                // Remove the image from the image repository
                using (new OperationContextScope(Channel.ToContextChannel()))
                {
                    ExecuteClientAction(null, CommandRuntime.ToString(), s => this.Channel.DeleteOSImage(s, this.ImageName));
                }

                if (this.DeleteVHD.IsPresent)
                {
                    // Remove the underlying VHD from the blob storage
                    Disks.RemoveVHD(this.Channel, CurrentSubscription.SubscriptionId, mediaLink);
                }
            }
            catch (ServiceManagementClientException ex)
            {
                this.WriteErrorDetails(ex);
            }
        }

        protected override void OnProcessRecord()
        {
            this.RemoveVMImageProcess();
        }
    }
}
