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
    using System.Linq;
    using System.Management.Automation;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using WindowsAzure.ServiceManagement;
    using Common;
    using Model;
    using Helpers;
    using Properties;

    /// <summary>
    /// Updates a persistent VM object with a provisioning configuration.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureProvisioningConfig", DefaultParameterSetName = "Windows"), OutputType(typeof(IPersistentVM))]
    public class AddAzureProvisioningConfigCommand : ProvisioningConfigurationCmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "Virtual Machine to update.")]
        [ValidateNotNullOrEmpty]
        [Alias("InputObject")]
        public IPersistentVM VM
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            var role = VM.GetInstance();

            if (Linux.IsPresent)
            {
                var provisioningConfiguration = role.ConfigurationSets
                    .OfType<LinuxProvisioningConfigurationSet>()
                    .SingleOrDefault();

                if (provisioningConfiguration == null)
                {
                    provisioningConfiguration = new LinuxProvisioningConfigurationSet();
                    role.ConfigurationSets.Add(provisioningConfiguration);
                }

                SetProvisioningConfiguration(provisioningConfiguration);
                provisioningConfiguration.HostName = role.RoleName;

                if (DisableSSH.IsPresent == false || NoSSHEndpoint.IsPresent)
                {
                    var netConfig = role.ConfigurationSets
                        .OfType<NetworkConfigurationSet>()
                        .SingleOrDefault();

                    if (netConfig == null)
                    {
                        netConfig = new NetworkConfigurationSet
                        {
                            InputEndpoints =
                                new System.Collections.ObjectModel.Collection<InputEndpoint>()
                        };

                        role.ConfigurationSets.Add(netConfig);
                    }

                    // Add check in case the settings were imported 
                    bool addSSH = true;

                    foreach (InputEndpoint ep in netConfig.InputEndpoints)
                    {
                        if (string.Compare(ep.Name, "SSH", StringComparison.OrdinalIgnoreCase) == 0 || ep.LocalPort == 22)
                        {
                            addSSH = false;
                            ep.Port = null; // null out to avoid conflicts
                            break;
                        }
                    }

                    if (addSSH)
                    {
                        InputEndpoint sshEndpoint = new InputEndpoint();
                        sshEndpoint.LocalPort = 22;
                        sshEndpoint.Protocol = "tcp";
                        sshEndpoint.Name = "SSH";
                        netConfig.InputEndpoints.Add(sshEndpoint);
                    }
                }
            }
            else
            {
                var provisioningConfiguration = role.ConfigurationSets
                    .OfType<WindowsProvisioningConfigurationSet>()
                    .SingleOrDefault();
                if (provisioningConfiguration == null)
                {
                    provisioningConfiguration = new WindowsProvisioningConfigurationSet();
                    role.ConfigurationSets.Add(provisioningConfiguration);
                }

                SetProvisioningConfiguration(provisioningConfiguration);
                provisioningConfiguration.ComputerName = role.RoleName;

                if (!NoRDPEndpoint.IsPresent)
                {
                    var netConfig = role.ConfigurationSets
                        .OfType<NetworkConfigurationSet>()
                        .SingleOrDefault();

                    if (netConfig == null)
                    {
                        netConfig = new NetworkConfigurationSet();
                        role.ConfigurationSets.Add(netConfig);
                    }

                    if (netConfig.InputEndpoints == null)
                    {
                        netConfig.InputEndpoints = new System.Collections.ObjectModel.Collection<InputEndpoint>();
                    }

                    bool addRDP = true;

                    foreach (InputEndpoint ep in netConfig.InputEndpoints)
                    {
                        if (string.Compare(ep.Name, "RDP", StringComparison.OrdinalIgnoreCase) == 0 || ep.LocalPort == 3389)
                        {
                            addRDP = false;
                            ep.Port = null; // null out to avoid conflicts
                            break;
                        }
                    }

                    if (addRDP)
                    {
                        InputEndpoint rdpEndpoint = new InputEndpoint { LocalPort = 3389, Protocol = "tcp", Name = "RDP" };
                        netConfig.InputEndpoints.Add(rdpEndpoint);
                    }
                }

                if (!this.DisableWinRMHttps.IsPresent)
                {
                    var netConfig = role.ConfigurationSets
                        .OfType<NetworkConfigurationSet>()
                        .SingleOrDefault();

                    if (netConfig == null)
                    {
                        netConfig = new NetworkConfigurationSet();
                        role.ConfigurationSets.Add(netConfig);
                    }

                    if (netConfig.InputEndpoints == null)
                    {
                        netConfig.InputEndpoints = new System.Collections.ObjectModel.Collection<InputEndpoint>();
                    }

                    var builder = new WinRmConfigurationBuilder();
                    if (this.EnableWinRMHttp.IsPresent)
                    {
                        builder.AddHttpListener();
                    }
                    builder.AddHttpsListener(this.WinRMCertificate);
                    provisioningConfiguration.WinRM = builder.Configuration;

                    var winRmEndpoint = new InputEndpoint {LocalPort = WinRMConstants.HttpsListenerPort, Protocol = "tcp", Name = WinRMConstants.EndpointName};
                    netConfig.InputEndpoints.Add(winRmEndpoint);
                    role.WinRMCertificate = WinRMCertificate;
                }

                role.X509Certificates = new List<X509Certificate2>();
                if (this.X509Certificates != null)
                {
                    role.X509Certificates.AddRange(this.X509Certificates);
                }
                role.NoExportPrivateKey = this.NoExportPrivateKey.IsPresent;
            }

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

        protected void ValidateParameters()
        {
            PersistentVM vm = (PersistentVM)this.VM;
            
            if (string.Compare(ParameterSetName, "Linux", StringComparison.OrdinalIgnoreCase) == 0 && ValidationHelpers.IsLinuxPasswordValid(Password) == false)
            {
                throw new ArgumentException(Resources.PasswordNotComplexEnough);
            }

            if ((string.Compare(ParameterSetName, "Windows", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(ParameterSetName, "WindowsDomain", StringComparison.OrdinalIgnoreCase) == 0) && ValidationHelpers.IsWindowsPasswordValid(Password) == false)
            {
                throw new ArgumentException(Resources.PasswordNotComplexEnough);
            }

            if (string.Compare(ParameterSetName, "Linux", StringComparison.OrdinalIgnoreCase) == 0 && ValidationHelpers.IsLinuxHostNameValid(vm.RoleName) == false)
            {
                throw new ArgumentException(Resources.InvalidHostName);
            }

            if ((string.Compare(ParameterSetName, "Windows", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(ParameterSetName, "WindowsDomain", StringComparison.OrdinalIgnoreCase) == 0) && ValidationHelpers.IsWindowsComputerNameValid(vm.RoleName) == false)
            {
                throw new ArgumentException(Resources.InvalidComputerName);
            }
        }
    }
}
