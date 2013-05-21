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
    using System.Net;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;
    using Properties;

    public class IaaSDeploymentManagementCmdletBase : ServiceManagementBaseCmdlet
    {
        private static IList<string> TerminalStates = new List<string>
        {
            "FailedStartingVM",
            "ProvisioningFailed",
            "ProvisioningTimeout"
        };

        public IaaSDeploymentManagementCmdletBase()
        {
            CurrentDeployment = null;
            GetDeploymentOperation = null;
            CreatingNewDeployment = false;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        public virtual string ServiceName
        {
            get;
            set;
        }

        protected Deployment CurrentDeployment { get; set; }

        protected Operation GetDeploymentOperation { get; set; }

        protected bool CreatingNewDeployment { get; set; }

        protected string GetDeploymentServiceName { get; set; }

        internal virtual void ExecuteCommand()
        {
            if (!string.IsNullOrEmpty(ServiceName))
            {
                InvokeInOperationContext(() =>
                {
                    try
                    {
                        WriteVerboseWithTimestamp(Resources.GetDeploymentBeginOperation);
                        CurrentDeployment = RetryCall(s => Channel.GetDeploymentBySlot(s, ServiceName, DeploymentSlotType.Production));
                        GetDeploymentOperation = GetOperation();
                        WriteVerboseWithTimestamp(Resources.GetDeploymentCompletedOperation);
                    }
                    catch (Exception e)
                    {
                        var we = e.InnerException as WebException;
                        if (we != null && ((HttpWebResponse)we.Response).StatusCode != HttpStatusCode.NotFound)
                        {
                            throw;
                        }
                    }
                });
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        protected void WaitForRoleToBoot(string roleName)
        {
            string currentInstanceStatus = null;
            RoleInstance durableRoleInstance;
            do
            {
                Deployment deployment = null;
                InvokeInOperationContext(() =>
                {
                    try
                    {
                        deployment = RetryCall(s => Channel.GetDeploymentBySlot(s, ServiceName, DeploymentSlotType.Production));
                    }
                    catch (Exception e)
                    {
                        var we = e.InnerException as WebException;
                        if (we != null && ((HttpWebResponse)we.Response).StatusCode != HttpStatusCode.NotFound)
                        {
                            throw;
                        }
                    }
                });

                if (deployment == null)
                {
                    throw new ApplicationException(String.Format(Resources.CouldNotFindDeployment, ServiceName, DeploymentSlotType.Production));
                }
                durableRoleInstance = deployment.RoleInstanceList.Find(ri => ri.RoleName == roleName);

                if (currentInstanceStatus == null)
                {
                    this.WriteVerboseWithTimestamp(Resources.RoleInstanceStatus, durableRoleInstance.InstanceStatus);
                    currentInstanceStatus = durableRoleInstance.InstanceStatus;
                }

                if (currentInstanceStatus != durableRoleInstance.InstanceStatus)
                {
                    this.WriteVerboseWithTimestamp(Resources.RoleInstanceStatus, durableRoleInstance.InstanceStatus);
                    currentInstanceStatus = durableRoleInstance.InstanceStatus;
                }

                if(TerminalStates.FirstOrDefault(r => String.Compare(durableRoleInstance.InstanceStatus, r, true, CultureInfo.InvariantCulture) == 0) != null)
                {
                    var message = string.Format(Resources.VMCreationFailedWithVMStatus, durableRoleInstance.InstanceStatus);
                    throw new ApplicationException(message);
                }
                
                if(String.Compare(durableRoleInstance.InstanceStatus, RoleInstanceStatus.ReadyRole, true, CultureInfo.InvariantCulture) == 0)
                {
                    break;
                }
                
                //Once we move Tasks, we should remove Thread.Sleep
                Thread.Sleep(TimeSpan.FromSeconds(30));

            } while (true);
        }
    }
}