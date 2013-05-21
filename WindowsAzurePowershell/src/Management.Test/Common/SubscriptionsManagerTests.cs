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
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common.XmlSchema;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SubscriptionsManagerTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureAppDir;
            Directory.CreateDirectory(GlobalPathInfo.GlobalSettingsDirectory);
        }

        [TestMethod]
        public void TestImportSubscriptions()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                var publishSettings = General.DeserializeXmlFile<PublishData>(Data.ValidPublishSettings[i]);
                var subscriptionsManager = SubscriptionsManager.Import(
                    Data.ValidSubscriptionsData[i],
                    publishSettings);

                // All subscriptions from both the publish settings file and the subscriptions file were imported
                Assert.AreEqual(6, subscriptionsManager.Subscriptions.Count);
                Assert.IsTrue(Data.ValidSubscriptionName.SequenceEqual(subscriptionsManager.Subscriptions.Keys));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestImportSubscriptionsInvalidSubscriptionData()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                try
                {
                    var publishSettings = General.DeserializeXmlFile<PublishData>(Data.ValidPublishSettings[i]);
                    SubscriptionsManager.Import(
                        Data.InvalidSubscriptionsData[i],
                        publishSettings);
                }
                catch (InvalidOperationException exception)
                {
                    Assert.AreEqual(
                        string.Format(Resources.InvalidSubscriptionsDataSchema, Data.InvalidSubscriptionsData[i]),
                        exception.Message);
                    throw;
                }
            }
        }

        [TestMethod]
        public void TestSaveSubscriptions()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings[i]);
                
                var subscriptionsManager = SubscriptionsManager.Import(
                    Data.ValidSubscriptionsData[i],
                    globalSettingsManager.PublishSettings,
                    globalSettingsManager.Certificate);

                var newSubscription = new SubscriptionData
                {
                    SubscriptionName = "newsubscription",
                    IsDefault = false,
                    SubscriptionId = "id"
                };

                subscriptionsManager.Subscriptions[newSubscription.SubscriptionName] = newSubscription;
                subscriptionsManager.SaveSubscriptions(Path.Combine(GlobalPathInfo.GlobalSettingsDirectory, "test.xml"));

                var newSubscriptionsManager = SubscriptionsManager.Import(
                    Path.Combine(GlobalPathInfo.GlobalSettingsDirectory, "test.xml"),
                    globalSettingsManager.PublishSettings,
                    globalSettingsManager.Certificate);

                var addedSubscription = newSubscriptionsManager.Subscriptions.Values.Single(
                    subscription => subscription.SubscriptionName == newSubscription.SubscriptionName);

                Assert.AreEqual(newSubscription.SubscriptionId, addedSubscription.SubscriptionId);

                globalSettingsManager.DeleteGlobalSettingsManager();
            }
        }
    }
}
