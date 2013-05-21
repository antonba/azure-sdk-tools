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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.Endpoints
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using IaaS;
    using WindowsAzure.ServiceManagement;
    using Model;
    using Properties;

    [Cmdlet(VerbsCommon.Add, "AzureEndpoint", DefaultParameterSetName = "NoLB"), OutputType(typeof(IPersistentVM))]
    public class AddAzureEndpoint : VirtualMachineConfigurationCmdletBase 
    {
        private const string NoLBParameterSet = "NoLB";
        private const string LoadBalancedParameterSet = "LoadBalanced";
        private const string LoadBalancedProbeParameterSet = "LoadBalancedProbe";

        [Parameter(Position = 0, ParameterSetName = NoLBParameterSet, Mandatory = true, HelpMessage = "Endpoint name")]
        [Parameter(Position = 0, ParameterSetName = LoadBalancedParameterSet, Mandatory = true, HelpMessage = "Endpoint name")]
        [Parameter(Position = 0, ParameterSetName = LoadBalancedProbeParameterSet, Mandatory = true, HelpMessage = "Endpoint name")]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = NoLBParameterSet, HelpMessage = "Endpoint protocol.")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = LoadBalancedParameterSet, HelpMessage = "Endpoint protocol.")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = LoadBalancedProbeParameterSet, HelpMessage = "Endpoint protocol.")]
        [ValidateSet("tcp", "udp", IgnoreCase = true)]
        [ValidateNotNullOrEmpty]
        public string Protocol
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = NoLBParameterSet, HelpMessage = "Local port.")]
        [Parameter(Position = 2, Mandatory = true, ParameterSetName = LoadBalancedParameterSet, HelpMessage = "Local port.")]
        [Parameter(Position = 2, Mandatory = true, ParameterSetName = LoadBalancedProbeParameterSet, HelpMessage = "Local port.")]
        [ValidateNotNullOrEmpty]
        public int LocalPort
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = NoLBParameterSet, HelpMessage = "Public port.")]
        [Parameter(Mandatory = false, ParameterSetName = LoadBalancedParameterSet, HelpMessage = "Public port.")]
        [Parameter(Mandatory = false, ParameterSetName = LoadBalancedProbeParameterSet, HelpMessage = "Public port.")]
        [ValidateNotNullOrEmpty]
        public int? PublicPort
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = LoadBalancedParameterSet, HelpMessage = "Load Balanced Endpoint Set Name")]
        [Parameter(Mandatory = true, ParameterSetName = LoadBalancedProbeParameterSet, HelpMessage = "Load Balanced Endpoint Set Name")]
        [Alias("LoadBalancedEndpointSetName")]
        [ValidateNotNullOrEmpty]
        public string LBSetName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = LoadBalancedProbeParameterSet, HelpMessage = "Probe Port")]
        public int ProbePort
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, ParameterSetName = LoadBalancedProbeParameterSet, HelpMessage = "Probe Protocol (http/tcp)")]
        [ValidateSet("tcp", "http", IgnoreCase = true)]
        public string ProbeProtocol
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = LoadBalancedProbeParameterSet, HelpMessage = "Probe Relative Path")]
        public string ProbePath
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = LoadBalancedProbeParameterSet, HelpMessage = "Probe Interval in Seconds.")]
        [ValidateNotNullOrEmpty]
        public int? ProbeIntervalInSeconds
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ParameterSetName = LoadBalancedProbeParameterSet, HelpMessage = "Probe Timeout in Seconds.")]
        [ValidateNotNullOrEmpty]
        public int? ProbeTimeoutInSeconds
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            ValidateParameters();

            var endpoints = GetInputEndpoints();
            var endpoint = endpoints.SingleOrDefault(p => p.Name == Name);

            if (endpoint != null)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                            new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.EndpointAlreadyDefinedForVM, this.Name)),
                            string.Empty,
                            ErrorCategory.InvalidData,
                            null));
            }

            endpoint = new InputEndpoint
            {
                Name = Name,
                Port = PublicPort.HasValue ? PublicPort : null,
                LocalPort = LocalPort,
                Protocol = Protocol,
            };

            if (ParameterSetName == LoadBalancedProbeParameterSet)
            {
                endpoint.LoadBalancedEndpointSetName = LBSetName;
                endpoint.LoadBalancerProbe = new LoadBalancerProbe { Protocol = ProbeProtocol };

                endpoint.LoadBalancerProbe.Port = ProbePort;

                if (endpoint.LoadBalancerProbe.Protocol == "http")
                {
                    if (string.IsNullOrEmpty(ProbePath) == false)
                    {
                        endpoint.LoadBalancerProbe.Path = ProbePath;
                    }
                    else
                    {
                        endpoint.LoadBalancerProbe.Path = "/";
                    }
                }

                if (ProbeIntervalInSeconds.HasValue)
                {
                    endpoint.LoadBalancerProbe.IntervalInSeconds = ProbeIntervalInSeconds;
                }

                if (ProbeTimeoutInSeconds.HasValue)
                {
                    endpoint.LoadBalancerProbe.TimeoutInSeconds = ProbeTimeoutInSeconds;
                }
            }
            else if (ParameterSetName == LoadBalancedParameterSet)
            {
                endpoint.LoadBalancedEndpointSetName = LBSetName;
            }

            endpoints.Add(endpoint);

            WriteObject(VM, true);
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

        protected Collection<InputEndpoint> GetInputEndpoints()
        {
            var role = VM.GetInstance();

            var networkConfiguration = role.ConfigurationSets
                                        .OfType<NetworkConfigurationSet>()
                                        .SingleOrDefault();

            if (networkConfiguration == null)
            {
                networkConfiguration = new NetworkConfigurationSet();
                role.ConfigurationSets.Add(networkConfiguration);
            }

            if (networkConfiguration.InputEndpoints == null)
            {
                networkConfiguration.InputEndpoints = new Collection<InputEndpoint>();
            }

            var inputEndpoints = networkConfiguration.InputEndpoints;

            return inputEndpoints;
        }

        private void ValidateParameters()
        {
            if (string.Compare(ParameterSetName, "LoadBalancedProbe", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (string.Compare(ProbeProtocol, "tcp", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!string.IsNullOrEmpty(ProbePath))
                    {
                        throw new ArgumentException(Resources.ProbePathIsNotValidWithTcp);
                    }
                }

                if (string.Compare(ProbeProtocol, "http", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (string.IsNullOrEmpty(ProbePath))
                    {
                        throw new ArgumentException(Resources.ProbePathIsRequiredForHttp);
                    }
                }
            }

            if (LocalPort < 1 || LocalPort > 65535)
            {
                throw new ArgumentException(Resources.PortSpecifiedIsNotInRange);
            }

            if (PublicPort != null && (PublicPort < 1 || PublicPort > 65535))
            {
                throw new ArgumentException(Resources.PortSpecifiedIsNotInRange);
            }

            if (ParameterSetName == "LoadBalancedProbe")
            {
                if (ProbePort < 1 || ProbePort > 65535)
                {
                    throw new ArgumentException(Resources.PortSpecifiedIsNotInRange);
                }
            }
        }       
    }
}