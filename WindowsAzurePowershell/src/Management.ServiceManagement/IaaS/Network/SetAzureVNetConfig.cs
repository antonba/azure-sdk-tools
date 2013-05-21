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
    using System.IO;
    using System.Management.Automation;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;
    using Properties;

    [Cmdlet(VerbsCommon.Set, "AzureVNetConfig"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureVNetConfigCommand : ServiceManagementBaseCmdlet
    {
        public SetAzureVNetConfigCommand()
        {
        }

        public SetAzureVNetConfigCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Path to the Network Configuration file (.xml).")]
        [ValidateNotNullOrEmpty]
        public string ConfigurationPath
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            ValidateParameters();

            FileStream netConfigFS = null;

            try
            {
                netConfigFS = new FileStream(this.ConfigurationPath, FileMode.Open);

                ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => this.Channel.SetNetworkConfiguration(s, netConfigFS));
            }
            finally
            {
                if (netConfigFS != null)
                {
                    netConfigFS.Close();
                }
            }
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }


        private void ValidateParameters()
        {
            if (!File.Exists(ConfigurationPath))
            {
                throw new ArgumentException(Resources.NetworkConfigurationFilePathDoesNotExist);
            }
        }
    }
}
