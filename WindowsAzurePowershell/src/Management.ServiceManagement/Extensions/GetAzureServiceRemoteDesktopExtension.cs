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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Get Windows Azure Service Remote Desktop Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureServiceRemoteDesktopExtension"), OutputType(typeof(IEnumerable<RemoteDesktopExtensionContext>))]
    public class GetAzureServiceRemoteDesktopExtensionCommand : BaseAzureServiceRemoteDesktopExtensionCmdlet
    {
        public GetAzureServiceRemoteDesktopExtensionCommand()
            : base()
        {
        }

        public GetAzureServiceRemoteDesktopExtensionCommand(IServiceManagement channel)
            : base(channel)
        {
        }

        [Parameter(Position = 0, Mandatory = false, HelpMessage = "Service Name")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, HelpMessage = "Deployment Slot: Production (default) or Staging")]
        [ValidateSet(DeploymentSlotType.Production, DeploymentSlotType.Staging, IgnoreCase = true)]
        public override string Slot
        {
            get;
            set;
        }

        protected override void ValidateParameters()
        {
            ValidateService();
            ValidateDeployment();
        }

        public void ExecuteCommand()
        {
            ValidateParameters();
            ExecuteClientActionInOCS(null,
                CommandRuntime.ToString(),
                s => this.Channel.ListHostedServiceExtensions(CurrentSubscription.SubscriptionId, ServiceName),
                (op, extensions) =>
                {
                    var extensionRoleList = (from r in Deployment.RoleList
                                             select new ExtensionRole(r.RoleName)).ToList().Union(new ExtensionRole[] { new ExtensionRole() });
                    return from role in extensionRoleList
                           from extension in extensions
                           where ExtensionManager.CheckExtensionType(extension.Id, ExtensionNameSpace, ExtensionType)
                              && ExtensionManager.GetBuilder(Deployment.ExtensionConfiguration).Exist(role, extension.Id)
                           select new RemoteDesktopExtensionContext
                           {
                               OperationId = op.OperationTrackingId,
                               OperationDescription = CommandRuntime.ToString(),
                               OperationStatus = op.Status,
                               Extension = extension.Type,
                               ProviderNameSpace = extension.ProviderNameSpace,
                               Id = extension.Id,
                               Role = role,
                               UserName = GetPublicConfigValue(extension, UserNameElemStr),
                               Expiration = GetPublicConfigValue(extension, ExpirationElemStr)
                           };
                });
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}
