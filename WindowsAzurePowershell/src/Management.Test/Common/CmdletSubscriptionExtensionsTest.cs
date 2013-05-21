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
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CmdletSubscriptionExtensionsTest
    {
        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();

            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureAppDir;

            if (Directory.Exists(Data.AzureAppDir))
            {
                Directory.Delete(Data.AzureAppDir, true);
            }
        }

        [TestMethod]
        public void TestGetSubscriptions()
        {
            var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var cmdletStub = new CmdletStub();
            var subscriptions = cmdletStub.GetSubscriptions(null);

            // All subscriptions from both the publish settings file and the subscriptions file were imported
            Assert.AreEqual(6, subscriptions.Count);

            // There's a single default subscription
            Assert.AreEqual("Windows Azure Sandbox 9-220", subscriptions.Values.Single(subscription => subscription.IsDefault).SubscriptionName);

            globalSettingsManager.DeleteGlobalSettingsManager();
        }

        [TestMethod]
        public void TestGetCurrentSubscription()
        {
            var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var cmdletStub = new CmdletStub();
            var subscriptions = cmdletStub.GetSubscriptions(null);

            var currentSubscription = subscriptions.Values.First();
            cmdletStub.SetCurrentSubscription(currentSubscription.SubscriptionName, null);

            // Test
            var actualCurrentSubscription = cmdletStub.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionName, actualCurrentSubscription.SubscriptionName);
            Assert.AreEqual(currentSubscription.SubscriptionId, actualCurrentSubscription.SubscriptionId);

            globalSettingsManager.DeleteGlobalSettingsManager();
        }

        [TestMethod]
        public void TestSetDefaultSubscription()
        {
            var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var newPath = Path.Combine(GlobalPathInfo.GlobalSettingsDirectory, "test.xml");
            File.Copy(Data.ValidSubscriptionsData[0], newPath, true);

            var cmdletStub = new CmdletStub();
            var subscriptions = cmdletStub.GetSubscriptions(Data.ValidSubscriptionsData[0]);

            var newDefaultSubscription = subscriptions.Values.First(subscription => !subscription.IsDefault);
            cmdletStub.SetDefaultSubscription(newDefaultSubscription.SubscriptionName, newPath);

            // Test - reimport and make sure the current subscription after import is the correct one
            var subscriptionsManager = GlobalSettingsManager.Load(GlobalPathInfo.GlobalSettingsDirectory, newPath).SubscriptionManager;
            var defaultSubscription = subscriptionsManager.Subscriptions.Values.First(subscription => subscription.IsDefault);
            Assert.AreEqual(newDefaultSubscription.SubscriptionName, defaultSubscription.SubscriptionName);

            globalSettingsManager.DeleteGlobalSettingsManager();
        }

        [TestMethod]
        public void TestUpdateSubscriptions()
        {
            var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var cmdletStub = new CmdletStub();
            var subscriptions = cmdletStub.GetSubscriptions(Data.ValidSubscriptionsData[0]);

            var deleteSubscriptionKey = subscriptions.Keys.First();
            subscriptions.Remove(deleteSubscriptionKey);

            var newPath = Path.Combine(GlobalPathInfo.GlobalSettingsDirectory, "test.xml");
            cmdletStub.UpdateSubscriptions(subscriptions, newPath);

            var newSubscriptions = cmdletStub.GetSubscriptions(newPath);
            Assert.IsFalse(newSubscriptions.ContainsKey(deleteSubscriptionKey));

            globalSettingsManager.DeleteGlobalSettingsManager();
        }

        [TestMethod]
        public void TestGetSubscription()
        {
            var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var cmdletStub = new CmdletStub();
            var subscription = cmdletStub.GetSubscription("TestSubscription1", null);

            Assert.AreEqual("TestSubscription1", subscription.SubscriptionName);

            globalSettingsManager.DeleteGlobalSettingsManager();
        }
    }

    public class CmdletStub : PSCmdlet
    {
    }
}
