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

namespace Microsoft.WindowsAzure.Management.Test.Common
{
    using System.IO;
    using System.Linq;
    using Microsoft.WindowsAzure.Management.Subscription;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RemoveSubscriptionTest
    {
        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();

            if (Directory.Exists(Data.AzureAppDir))
            {
                Directory.Delete(Data.AzureAppDir, true);
            }
        }

        [TestMethod]
        public void TestRemoveDefaultSubscriptionProcess()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                var targetFile = Path.Combine(Directory.GetParent(Data.ValidSubscriptionsData[i]).FullName, "removeonce" + Path.GetFileName(Data.ValidSubscriptionsData[i]));
                File.Copy(Data.ValidSubscriptionsData[i], targetFile, true);
                var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, targetFile, Data.ValidPublishSettings[i]);
                MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
                var removeSubscriptionCommand = new RemoveAzureSubscriptionCommand();
                removeSubscriptionCommand.CommandRuntime = mockCommandRuntime;
                removeSubscriptionCommand.RemoveSubscriptionProcess("mysub1", targetFile);

                var subscriptionsManager = SubscriptionsManager.Import(targetFile);
                Assert.IsFalse(subscriptionsManager.Subscriptions.Values.Any(subscription => subscription.SubscriptionName == "mysub1"));
                Assert.IsFalse(subscriptionsManager.Subscriptions.Values.Any(subscription => subscription.IsDefault));

                // Clean
                globalSettingsManager.DeleteGlobalSettingsManager();
            }
        }

        [TestMethod]
        public void TestRemoveNonDefaultSubscriptionProcess()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                var targetFile = Path.Combine(Directory.GetParent(Data.ValidSubscriptionsData[i]).FullName, "removeagain" + Path.GetFileName(Data.ValidSubscriptionsData[i]));
                File.Copy(Data.ValidSubscriptionsData[i], targetFile, true);
                var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, targetFile, Data.ValidPublishSettings[i]);
                MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
                var removeSubscriptionCommand = new RemoveAzureSubscriptionCommand();
                removeSubscriptionCommand.CommandRuntime = mockCommandRuntime;
                removeSubscriptionCommand.RemoveSubscriptionProcess("mysub2", targetFile);

                var subscriptionsManager = GlobalSettingsManager.Load(GlobalPathInfo.GlobalSettingsDirectory, targetFile);
                Assert.IsFalse(subscriptionsManager.Subscriptions.Values.Any(subscription => subscription.SubscriptionName == "mysub2"));
                Assert.IsTrue(subscriptionsManager.Subscriptions.Values.Any(subscription => subscription.IsDefault));

                // Clean
                globalSettingsManager.DeleteGlobalSettingsManager();
            }
        }

        [TestMethod]
        public void TestRemoveCurrentSubscriptionProcess()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                var targetFile = Path.Combine(Directory.GetParent(Data.ValidSubscriptionsData[i]).FullName, "removeonce" + Path.GetFileName(Data.ValidSubscriptionsData[i]));
                File.Copy(Data.ValidSubscriptionsData[i], targetFile, true);
                var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, targetFile, Data.ValidPublishSettings[i]);
                MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
                
                var removeSubscriptionCommand = new RemoveAzureSubscriptionCommand();
                removeSubscriptionCommand.SetCurrentSubscription(globalSettingsManager.Subscriptions["mysub1"]);
                removeSubscriptionCommand.CommandRuntime = mockCommandRuntime;
                removeSubscriptionCommand.RemoveSubscriptionProcess("mysub1", targetFile);

                var subscriptionsManager = SubscriptionsManager.Import(targetFile);
                Assert.IsFalse(subscriptionsManager.Subscriptions.Values.Any(subscription => subscription.SubscriptionName == "mysub1"));
                Assert.IsFalse(subscriptionsManager.Subscriptions.Values.Any(subscription => subscription.IsDefault));
                Assert.IsNull(removeSubscriptionCommand.GetCurrentSubscription());

                // Clean
                globalSettingsManager.DeleteGlobalSettingsManager();
            }
        }
    }
}
