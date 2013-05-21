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

namespace Microsoft.WindowsAzure.Management.Test.CloudService.Utilities
{
    using System;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;
    using PHPRuntime = Microsoft.WindowsAzure.Management.Utilities.CloudService.CloudRuntime.PHPCloudRuntime;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;

    [TestClass]
    public class AzureCloudRuntimeTests : TestBase
    {
        [TestMethod]
        public void GetInstalledRuntimeVersionWithLocalPHPTest()
        {
            PHPRuntime runtime = new PHPRuntime();
            string detversion = runtime.GetInstalledRuntimeVersion();
            string envversion = Environment.GetEnvironmentVariable(Resources.RuntimeVersionInstalled);
            string defversion = Resources.PHPDefaultRuntimeVersion;

            if (envversion != "")
                Assert.IsTrue(detversion == envversion);
        }

        [TestMethod]
        public void GetInstalledRuntimeVersionWithoutLocalPHPTest()
        {
            PHPRuntime runtime = new PHPRuntime();
            string detversion = runtime.GetInstalledRuntimeVersion();
            string envversion = Environment.GetEnvironmentVariable(Resources.RuntimeVersionInstalled);
            string defversion = Resources.PHPDefaultRuntimeVersion;
            
            if (envversion == "")
                Assert.IsTrue(detversion == defversion);
        }
   }
}