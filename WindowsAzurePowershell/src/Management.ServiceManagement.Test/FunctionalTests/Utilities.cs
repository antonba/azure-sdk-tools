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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Test.FunctionalTests
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Test.Properties;



    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
    using System.Threading;
    using Sync.Download;

    using Microsoft.WindowsAzure.Storage.Blob;

    internal class Utilities 
    {
        public static string windowsAzurePowershellPath = Path.Combine(Environment.CurrentDirectory);
        public const string windowsAzurePowershellModuleCloudService = "Microsoft.WindowsAzure.Management.CloudService.dll";
        public const string windowsAzurePowershellModuleManagement = "Microsoft.WindowsAzure.Management.dll";
        public const string windowsAzurePowershellModuleService = "Microsoft.WindowsAzure.Management.Service.dll";
        public const string windowsAzurePowershellModuleServiceManagement = "Microsoft.WindowsAzure.Management.ServiceManagement.dll";
                
        // AzureAffinityGroup
        public const string NewAzureAffinityGroupCmdletName = "New-AzureAffinityGroup";
        public const string GetAzureAffinityGroupCmdletName = "Get-AzureAffinityGroup";
        public const string SetAzureAffinityGroupCmdletName = "Set-AzureAffinityGroup";
        public const string RemoveAzureAffinityGroupCmdletName = "Remove-AzureAffinityGroup";

        // AzureAvailablitySet
        public const string SetAzureAvailabilitySetCmdletName = "Set-AzureAvailabilitySet";

        // AzureCertificate & AzureCertificateSetting
        public const string AddAzureCertificateCmdletName = "Add-AzureCertificate";
        public const string GetAzureCertificateCmdletName = "Get-AzureCertificate";
        public const string RemoveAzureCertificateCmdletName = "Remove-AzureCertificate";
        public const string NewAzureCertificateSettingCmdletName = "New-AzureCertificateSetting";


        // AzureDataDisk
        public const string AddAzureDataDiskCmdletName = "Add-AzureDataDisk";
        public const string GetAzureDataDiskCmdletName = "Get-AzureDataDisk";
        public const string SetAzureDataDiskCmdletName = "Set-AzureDataDisk";        
        public const string RemoveAzureDataDiskCmdletName = "Remove-AzureDataDisk";

        // AzureDeployment
        public const string NewAzureDeploymentCmdletName = "New-AzureDeployment";
        public const string GetAzureDeploymentCmdletName = "Get-AzureDeployment";
        public const string SetAzureDeploymentCmdletName = "Set-AzureDeployment";
        public const string RemoveAzureDeploymentCmdletName = "Remove-AzureDeployment";
        public const string MoveAzureDeploymentCmdletName = "Move-AzureDeployment";

        // AzureDisk        
        public const string AddAzureDiskCmdletName = "Add-AzureDisk";
        public const string GetAzureDiskCmdletName = "Get-AzureDisk";
        public const string UpdateAzureDiskCmdletName = "Update-AzureDisk";
        public const string RemoveAzureDiskCmdletName = "Remove-AzureDisk";


        // AzureDns
        public const string NewAzureDnsCmdletName = "New-AzureDns";
        public const string GetAzureDnsCmdletName = "Get-AzureDns";

        // AzureEndpoint
        public const string AddAzureEndpointCmdletName = "Add-AzureEndpoint";        
        public const string GetAzureEndpointCmdletName = "Get-AzureEndpoint";
        public const string SetAzureEndpointCmdletName = "Set-AzureEndpoint";
        public const string RemoveAzureEndpointCmdletName = "Remove-AzureEndpoint";



        // AzureLocation
        public const string GetAzureLocationCmdletName = "Get-AzureLocation";
        

        // AzureOSDisk & AzureOSVersion
        public const string GetAzureOSDiskCmdletName = "Get-AzureOSDisk";
        public const string SetAzureOSDiskCmdletName = "Set-AzureOSDisk";

        public const string GetAzureOSVersionCmdletName = "Get-AzureOSVersion";

        // AzureProvisioningConfig
        public const string AddAzureProvisioningConfigCmdletName = "Add-AzureProvisioningConfig";

        // AzurePublishSettingsFile
        public const string ImportAzurePublishSettingsFileCmdletName = "Import-AzurePublishSettingsFile";
        public const string GetAzurePublishSettingsFileCmdletName = "Get-AzurePublishSettingsFile";

        // AzureQuickVM
        public const string NewAzureQuickVMCmdletName = "New-AzureQuickVM";

        // AzureRemoteDesktopFile
        public const string GetAzureRemoteDesktopFileCmdletName = "Get-AzureRemoteDesktopFile";
        


        // AzureRole & AzureRoleInstnace
        public const string GetAzureRoleCmdletName = "Get-AzureRole";
        public const string SetAzureRoleCmdletName = "Set-AzureRole";

        public const string GetAzureRoleInstanceCmdletName = "Get-AzureRoleInstance";

        // AzureService
        public const string NewAzureServiceCmdletName = "New-AzureService";
        public const string GetAzureServiceCmdletName = "Get-AzureService";
        public const string SetAzureServiceCmdletName = "Set-AzureService";
        public const string RemoveAzureServiceCmdletName = "Remove-AzureService";
        // AzureServiceDesktopExtension
        public const string SetAzureServiceRemoteDesktopExtensionCmdletName = "Set-AzureServiceRemoteDesktopExtension";
        
        // AzureSSHKey
        public const string NewAzureSSHKeyCmdletName = "New-AzureSSHKey";

        // AzureStorageAccount
        public const string NewAzureStorageAccountCmdletName = "New-AzureStorageAccount";
        public const string GetAzureStorageAccountCmdletName = "Get-AzureStorageAccount";        
        public const string SetAzureStorageAccountCmdletName = "Set-AzureStorageAccount";        
        public static string RemoveAzureStorageAccountCmdletName = "Remove-AzureStorageAccount";


        // AzureStorageKey
        public static string NewAzureStorageKeyCmdletName = "New-AzureStorageKey";
        public static string GetAzureStorageKeyCmdletName = "Get-AzureStorageKey";

        // AzureSubnet
        public static string GetAzureSubnetCmdletName = "Get-AzureSubnet";
        public static string SetAzureSubnetCmdletName = "Set-AzureSubnet";


        // AzureSubscription
        public const string GetAzureSubscriptionCmdletName = "Get-AzureSubscription";
        public const string SetAzureSubscriptionCmdletName = "Set-AzureSubscription";
        public const string SelectAzureSubscriptionCmdletName = "Select-AzureSubscription";
        public const string RemoveAzureSubscriptionCmdletName = "Remove-AzureSubscription";        


        // AzureVhd
        public static string AddAzureVhdCmdletName = "Add-AzureVhd";
        public static string SaveAzureVhdCmdletName = "Save-AzureVhd";

        // AzureVM
        public const string NewAzureVMCmdletName = "New-AzureVM";
        public const string GetAzureVMCmdletName = "Get-AzureVM";
        public const string UpdateAzureVMCmdletName = "Update-AzureVM";                
        public const string RemoveAzureVMCmdletName = "Remove-AzureVM";
        
        public const string ExportAzureVMCmdletName = "Export-AzureVM";
        public const string ImportAzureVMCmdletName = "Import-AzureVM";
        
        public const string StartAzureVMCmdletName = "Start-AzureVM";
        public const string StopAzureVMCmdletName = "Stop-AzureVM";
        public const string RestartAzureVMCmdletName = "Restart-AzureVM";
        
        
        

        // AzureVMConfig
        public const string NewAzureVMConfigCmdletName = "New-AzureVMConfig";

        // AzureVMImage
        
        public const string AddAzureVMImageCmdletName = "Add-AzureVMImage";
        public const string GetAzureVMImageCmdletName = "Get-AzureVMImage";
        public const string RemoveAzureVMImageCmdletName = "Remove-AzureVMImage";
        public const string SaveAzureVMImageCmdletName = "Save-AzureVMImage";
        public const string UpdateAzureVMImageCmdletName = "Update-AzureVMImage";
        
        // AzureVMSize
        public const string SetAzureVMSizeCmdletName = "Set-AzureVMSize";

        // AzureVNetConfig & AzureVNetConnection
        public const string GetAzureVNetConfigCmdletName = "Get-AzureVNetConfig";
        public const string SetAzureVNetConfigCmdletName = "Set-AzureVNetConfig";
        public const string RemoveAzureVNetConfigCmdletName = "Remove-AzureVNetConfig";
        
        public const string GetAzureVNetConnectionCmdletName = "Get-AzureVNetConnection";

        // AzureVnetGateway & AzureVnetGatewayKey
        public const string NewAzureVNetGatewayCmdletName = "New-AzureVNetGateway";
        public const string GetAzureVNetGatewayCmdletName = "Get-AzureVNetGateway";
        public const string SetAzureVNetGatewayCmdletName = "Set-AzureVNetGateway";
        public const string RemoveAzureVNetGatewayCmdletName = "Remove-AzureVNetGateway";

        public const string GetAzureVNetGatewayKeyCmdletName = "Get-AzureVNetGatewayKey";

        // AzureVNetSite
        public const string GetAzureVNetSiteCmdletName = "Get-AzureVNetSite";

        // AzureWalkUpgradeDomain
        public const string SetAzureWalkUpgradeDomainCmdletName = "Set-AzureWalkUpgradeDomain";


        public const string GetModuleCmdletName = "Get-Module";       
        public const string TestAzureNameCmdletName = "Test-AzureName";
        
        public const string CopyAzureStorageBlobCmdletName = "Copy-AzureStorageBlob";
        

        public static string GetUniqueShortName(string prefix = "", int length = 6, string suffix = "")
        {
            return string.Format("{0}{1}{2}", prefix, Guid.NewGuid().ToString("N").Substring(0, length), suffix);
        }

        public static int MatchKeywords(string input, string[] keywords, bool exactMatch = true)
        { //returns -1 for no match, 0 for exact match, and a positive number for how many keywords are matched.
            int result = 0;
            if (string.IsNullOrEmpty(input) || keywords.Length == 0)
                return -1;
            foreach (string keyword in keywords)
            {
                //For whole word match, modify pattern to be "\b{0}\b"
                if (!string.IsNullOrEmpty(keyword) && Regex.IsMatch(input, string.Format(@"{0}", Regex.Escape(keyword)), RegexOptions.IgnoreCase))
                {
                    result++;
                }
            }
            if (result == keywords.Length)
            {
                return 0;
            }
            else if (result == 0)
            {
                return -1;
            }
            else
            {
                if (exactMatch)
                {
                    return -1;
                }
                else
                {
                    return result;
                }
            }
        }

        public static Uri GetDeploymentAndWaitForReady(string serviceName, string slot, int waitTime, int maxWaitTime)
        {

            ServiceManagementCmdletTestHelper vmPowershellCmdlets = new ServiceManagementCmdletTestHelper();            
                       
            DateTime startTime = DateTime.Now;
            while (true)
            {
                bool allReady = true;
                DeploymentInfoContext result = vmPowershellCmdlets.GetAzureDeployment(serviceName, slot);
                int instanceNum = result.RoleInstanceList.Count;
                bool[] isReady = new bool[instanceNum];

                for (int j = 0; j < instanceNum; j++)
                {
                    var instance = result.RoleInstanceList[j];
                    Console.WriteLine("Instance: {0}, Status: {1}", instance.InstanceName, instance.InstanceStatus);
                    isReady[j] = (instance.InstanceStatus == "ReadyRole");
                    allReady &= isReady[j];
                }

                if (!allReady && (DateTime.Now - startTime).TotalSeconds < maxWaitTime)
                {
                    Console.WriteLine("Some roles are not ready, waiting for {0} seconds.", waitTime);
                    Thread.Sleep(waitTime*1000);
                }
                else if (!allReady) // some roles are not ready, and time-out.
                {
                    Assert.Fail("Deployment is not ready within {0} seconds!", maxWaitTime);
                }
                else // all roles are ready
                {
                    Console.WriteLine("Result of the deployment: {0}", result.Status);                    
                    return result.Url;
                }
            }
            
        }
        

        public static bool PrintAndCompareDeployment
            (DeploymentInfoContext deployment, string serviceName, string deploymentName, string deploymentLabel, string slot, string status, int instanceCount)
        {
            Console.WriteLine("ServiceName:{0}, DeploymentID: {1}, Uri: {2}", deployment.ServiceName, deployment.DeploymentId, deployment.Url.AbsoluteUri);
            Console.WriteLine("Name - {0}, Label - {1}, Slot - {2}, Status - {3}",
                deployment.DeploymentName, deployment.Label, deployment.Slot, deployment.Status);
            Console.WriteLine("RoleInstance: {0}", deployment.RoleInstanceList.Count);
            foreach (var instance in deployment.RoleInstanceList)
            {
                Console.WriteLine("InstanceName - {0}, InstanceStatus - {1}", instance.InstanceName, instance.InstanceStatus);
            }


            Assert.AreEqual(deployment.ServiceName, serviceName);
            Assert.AreEqual(deployment.DeploymentName, deploymentName);
            Assert.AreEqual(deployment.Label, deploymentLabel);
            Assert.AreEqual(deployment.Slot, slot);
            if (status != null)
            {
                Assert.AreEqual(deployment.Status, status);
            }

            Assert.AreEqual(deployment.RoleInstanceList.Count, instanceCount);

            return true;
        }

        // CheckRemove checks if 'fn(name)' exists.    'fn(name)' is usually 'Get-AzureXXXXX name'
        public static bool CheckRemove<Arg, Ret>(Func<Arg, Ret> fn, Arg name)
        {
            try
            {
                fn(name);
                return false;
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("ResourceNotFound"))
                {
                    Console.WriteLine("{0} does not exist.", name);
                    return true;
                }
                else
                {
                    Console.WriteLine("Error: {0}", e.ToString());
                    return false;
                }
            }
        }

        // CheckRemove checks if 'fn(name)' exists.    'fn(name)' is usually 'Get-AzureXXXXX name'
        public static bool CheckRemove<Arg1, Arg2, Ret>(Func<Arg1, Arg2, Ret> fn, Arg1 name1, Arg2 name2)
        {
            try
            {
                fn(name1, name2);
                return false;
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("ResourceNotFound"))
                {
                    Console.WriteLine("{0}, {1} is successfully removed", name1, name2);
                    return true;
                }
                else
                {
                    Console.WriteLine("Error: {0}", e.ToString());
                    return false;
                }
            }
        }

        // CheckRemove checks if 'fn(name)' exists.    'fn(name)' is usually 'Get-AzureXXXXX name'
        public static bool CheckRemove<Arg1, Arg2, Arg3, Ret>(Func<Arg1, Arg2, Arg3, Ret> fn, Arg1 name1, Arg2 name2, Arg3 name3)
        {
            try
            {
                fn(name1, name2, name3);
                return false;
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("ResourceNotFound"))
                {
                    Console.WriteLine("{0}, {1}, {2} is successfully removed", name1, name2, name3);
                    return true;
                }
                else
                {
                    Console.WriteLine("Error: {0}", e.ToString());
                    return false;
                }
            }
        }

        public static BlobHandle GetBlobHandle(string blob, string key)
        {
            BlobUri blobPath;
            Assert.IsTrue(BlobUri.TryParseUri(new Uri(blob), out blobPath));
            return new BlobHandle(blobPath, key);
        }
        public static bool RetryUntilSuccess <T> (Func<T> fn, string errorMessage, int maxTry, int intervalSeconds)
        {
            int i = 0;
            while (i < maxTry)
            {
                try
                {

                    fn();
                    return true;
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains(errorMessage))
                    {
                        Thread.Sleep(intervalSeconds * 1000);
                        i++;
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return false;            
        }
    }
}
