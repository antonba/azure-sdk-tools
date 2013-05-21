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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
    using System;
    using System.Management.Automation;
    using System.ServiceModel;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;


    [Cmdlet(VerbsCommon.Remove, "AzureDisk"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureDiskCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the data disk in the disk library to remove.")]
        [ValidateNotNullOrEmpty]
        public string DiskName
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

        public void RemoveDiskProcess()
        {
            try
            {            
                Uri mediaLink = null;
                if (this.DeleteVHD.IsPresent)
                {
                    // Get the location of the underlying VHD
                    using (new OperationContextScope(Channel.ToContextChannel()))
                    {
                        var disk = this.RetryCall(s => this.Channel.GetDisk(s, this.DiskName));
                        mediaLink = disk.MediaLink;
                    }
                }

                // Remove the disk from the disk repository
                using (new OperationContextScope(Channel.ToContextChannel()))
                {
                    ExecuteClientAction(null, CommandRuntime.ToString(), s => this.Channel.DeleteDisk(s, this.DiskName));
                }

                if (this.DeleteVHD.IsPresent)
                {
                    // Remove the underlying VHD from the blob storage
                    Helpers.Disks.RemoveVHD(this.Channel, CurrentSubscription.SubscriptionId, mediaLink);
                }
            }
            catch (ServiceManagementClientException ex)
            {
                this.WriteErrorDetails(ex);
            }
        }

        protected override void OnProcessRecord()
        {
            this.RemoveDiskProcess();
        }
    }
}