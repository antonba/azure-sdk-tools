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
    using System.Net;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.ServiceModel;
    using Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;
    using Properties;

    [Cmdlet(VerbsCommon.Get, "AzureVNetSite"), OutputType(typeof(IEnumerable<VirtualNetworkSiteContext>))]
    public class GetAzureVNetSiteCommand : ServiceManagementBaseCmdlet
    {
        public GetAzureVNetSiteCommand()
        {
        }

        public GetAzureVNetSiteCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, HelpMessage = "The virtual network name.")]
        [ValidateNotNullOrEmpty]
        public string VNetName
        {
            get;
            set;
        }

        public IEnumerable<VirtualNetworkSiteContext> GetVirtualNetworkSiteProcess()
        {            
            using (new OperationContextScope(Channel.ToContextChannel()))
            {
                try
                {
                    WriteVerboseWithTimestamp(string.Format(Resources.AzureVNetSiteBeginOperation, CommandRuntime.ToString()));

                    var sites = this.RetryCall(s => this.Channel.ListVirtualNetworkSites(s)).ToList();

                    if (!string.IsNullOrEmpty(this.VNetName))
                    {
                        sites = sites.Where(s => string.Equals(s.Name, this.VNetName, StringComparison.InvariantCultureIgnoreCase)).ToList();

                        if (sites.Count() == 0)
                        {
                            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.VirtualNetworkNameNotFound, this.VNetName), "VirtualNetworkName");
                        }
                    }

                    Operation operation = GetOperation();

                    WriteVerboseWithTimestamp(string.Format(Resources.AzureVNetSiteCompletedOperation, CommandRuntime.ToString()));

                    return sites.Select(s => new VirtualNetworkSiteContext
                    {
                        OperationId = operation.OperationTrackingId,
                        OperationDescription = CommandRuntime.ToString(),
                        OperationStatus = operation.Status,
                        AddressSpacePrefixes = s.AddressSpace != null ? s.AddressSpace.AddressPrefixes : null,
                        AffinityGroup = s.AffinityGroup,
                        DnsServers = s.Dns != null ? s.Dns.DnsServers : null,
                        GatewayProfile = s.Gateway != null ? s.Gateway.Profile : null,
                        GatewaySites = s.Gateway != null ? s.Gateway.Sites : null,
                        Id = s.Id,
                        InUse = s.InUse,
                        Label = s.Label,
                        Name = s.Name,
                        State = s.State,
                        Subnets = s.Subnets
                    }).ToList();
                }
                catch (ServiceManagementClientException ex)
                {
                    if (ex.HttpStatus == HttpStatusCode.NotFound && !IsVerbose())
                    {
                        return null;
                    }
                    else
                    {
                        this.WriteErrorDetails(ex);
                    }
                }

                return null;
            }
        }

        protected override void OnProcessRecord()
        {
            var virtualNetworkSites = this.GetVirtualNetworkSiteProcess();

            if (virtualNetworkSites != null)
            {
                WriteObject(virtualNetworkSites, true);
            }
        }
    }
}