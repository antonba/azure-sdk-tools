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

namespace Microsoft.WindowsAzure.Management.Test.Subscription
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.WindowsAzure.Management.Subscription;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetSubscriptionTest
    {
        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();

            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureAppDir;
        }

        [TestMethod]
        public void TestGetCurrentSubscriptionByName()
        {
            var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var importSubscriptionCommand = new ImportAzurePublishSettingsCommand();
            importSubscriptionCommand.ImportSubscriptionFile(
                Data.ValidPublishSettings.First(),
                null);

            var currentSubscription = importSubscriptionCommand.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionName, Data.Subscription1);
            Assert.IsTrue(currentSubscription.IsDefault);

            // Test the get for all subscription (null name)
            var getSubscriptionCommand = new GetSubscriptionCommandStub();
            getSubscriptionCommand.GetSubscriptionProcess("ByName", null, null);

            Assert.AreEqual(6, getSubscriptionCommand.Messages.Count);

            // Test the get for a specific susbcription
            getSubscriptionCommand = new GetSubscriptionCommandStub();
            getSubscriptionCommand.GetSubscriptionProcess("ByName", currentSubscription.SubscriptionName, null);

            Assert.AreEqual(1, getSubscriptionCommand.Messages.Count);
            Assert.AreEqual(currentSubscription.SubscriptionName, getSubscriptionCommand.Messages.First().SubscriptionName);
            Assert.AreEqual(currentSubscription.SubscriptionId, getSubscriptionCommand.Messages.First().SubscriptionId);

            globalSettingsManager.DeleteGlobalSettingsManager();
        }

        [TestMethod]
        public void TestGetCurrentSubscriptionCurrent()
        {
            var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var importSubscriptionCommand = new ImportAzurePublishSettingsCommand();
            importSubscriptionCommand.ImportSubscriptionFile(
                Data.ValidPublishSettings.First(),
                null);

            var currentSubscription = importSubscriptionCommand.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionName, Data.Subscription1);
            Assert.IsTrue(currentSubscription.IsDefault);

            // Test the get for the current subscription
            var getSubscriptionCommand = new GetSubscriptionCommandStub();
            getSubscriptionCommand.GetSubscriptionProcess("Current", null, null);

            Assert.AreEqual(1, getSubscriptionCommand.Messages.Count);
            Assert.AreEqual(currentSubscription.SubscriptionName, getSubscriptionCommand.Messages.First().SubscriptionName);
            Assert.AreEqual(currentSubscription.SubscriptionId, getSubscriptionCommand.Messages.First().SubscriptionId);

            globalSettingsManager.DeleteGlobalSettingsManager();
        }

        [TestMethod]
        public void TestGetCurrentSubscriptionDefault()
        {
            var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            var importSubscriptionCommand = new ImportAzurePublishSettingsCommand();
            importSubscriptionCommand.ImportSubscriptionFile(
                Data.ValidPublishSettings.First(),
                null);

            var currentSubscription = importSubscriptionCommand.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionName, Data.Subscription1);
            Assert.IsTrue(currentSubscription.IsDefault);

            // Test the get for the current subscription
            var getSubscriptionCommand = new GetSubscriptionCommandStub();
            getSubscriptionCommand.GetSubscriptionProcess("Default", null, null);

            Assert.AreEqual(1, getSubscriptionCommand.Messages.Count);
            Assert.AreEqual(currentSubscription.SubscriptionName, getSubscriptionCommand.Messages.First().SubscriptionName);
            Assert.AreEqual(currentSubscription.SubscriptionId, getSubscriptionCommand.Messages.First().SubscriptionId);

            globalSettingsManager.DeleteGlobalSettingsManager();
        }
    }

    public class GetSubscriptionCommandStub : GetAzureSubscriptionCommand
    {
        public readonly IList<SubscriptionData> Messages = new List<SubscriptionData>();

        protected override void WriteSubscription(SubscriptionData subscription)
        {
            Messages.Add(subscription);
        }
    }
}
