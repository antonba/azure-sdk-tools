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


namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.PersistentVMs
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.ServiceModel;
    using IaaS;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;
    using Properties;


    [Cmdlet(VerbsCommon.Get, "AzureRemoteDesktopFile", DefaultParameterSetName = "Download"), OutputType(typeof(ManagementOperationContext))]
    public class GetAzureRemoteDesktopFileCommand : IaaSDeploymentManagementCmdletBase
    {
        public GetAzureRemoteDesktopFileCommand()
        {
        }

        public GetAzureRemoteDesktopFileCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Name of the Role Instance or Virtual Machine Name to create/connect via RDP")]
        [ValidateNotNullOrEmpty]
        [Alias("InstanceName")]
        public string Name
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Path and name of the output RDP file.", ParameterSetName = "Download")]
        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Path and name of the output RDP file.", ParameterSetName = "Launch")]
        [ValidateNotNullOrEmpty]
        public string LocalPath
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, HelpMessage = "Start a remote desktop session to the specified role instance.", ParameterSetName = "Launch")]
        public SwitchParameter Launch
        {
            get;
            set;
        }

        [SecurityPermission(SecurityAction.LinkDemand)]
        internal override void ExecuteCommand()
        {
            base.ExecuteCommand();
            if (CurrentDeployment == null)
            {
                throw new ArgumentException(Resources.NoCloudServicePresent);
            }

            ManagementOperationContext context = null;

            string rdpFilePath = LocalPath ?? Path.GetTempFileName();
            using (new OperationContextScope(Channel.ToContextChannel()))
            {
                WriteVerboseWithTimestamp(string.Format(Resources.AzureRemoteDesktopBeginOperation, CommandRuntime.ToString()));

                using (var stream = RetryCall(s => Channel.DownloadRDPFile(s, ServiceName, CurrentDeployment.Name, Name + "_IN_0")))
                {
                    using (var file = File.Create(rdpFilePath))
                    {
                        int count;
                        byte[] buffer = new byte[1000];

                        while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            file.Write(buffer, 0, count);
                        }
                    }

                    Operation operation = GetOperation();

                    WriteVerboseWithTimestamp(string.Format(Resources.AzureRemoteDesktopCompletedOperation, CommandRuntime.ToString()));

                    context = new ManagementOperationContext
                                  {
                                      OperationDescription = CommandRuntime.ToString(),
                                      OperationStatus = operation.Status,
                                      OperationId = operation.OperationTrackingId
                                  };
                }
            }
            if (Launch.IsPresent)
            {
                var startInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                        
                if (LocalPath == null)
                {
                    string scriptGuid = Guid.NewGuid().ToString();

                    string launchRDPScript = Path.GetTempPath() + scriptGuid + ".bat";
                    using (var scriptStream = File.OpenWrite(launchRDPScript))
                    {
                        var writer = new StreamWriter(scriptStream);
                        writer.WriteLine("start /wait mstsc.exe " + rdpFilePath);
                        writer.WriteLine("del " + rdpFilePath);
                        writer.WriteLine("del " + launchRDPScript);
                        writer.Flush();
                    }

                    startInfo.FileName = launchRDPScript;
                }
                else
                {
                    startInfo.FileName = "mstsc.exe";
                    startInfo.Arguments = rdpFilePath;
                }

                Process.Start(startInfo);
            }

            WriteObject(context, true);
        }
    }
}