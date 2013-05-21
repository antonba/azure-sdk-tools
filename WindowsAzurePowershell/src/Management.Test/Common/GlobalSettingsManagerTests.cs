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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.Management.Subscription;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common.XmlSchema;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GlobalSettingsManagerTests
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
        public void GlobalSettingsManagerLoadExisting()
        {
            for (var i = 0; i < Data.ValidPublishSettings.Count; i++)
            {
                var publishSettingsFile = Data.ValidPublishSettings[i];

                // Prepare
                new ImportAzurePublishSettingsCommand().ImportSubscriptionFile(publishSettingsFile, null);
                GlobalSettingsManager globalSettingsManager = GlobalSettingsManager.Load(Data.AzureAppDir);
                PublishData actualPublishSettings = General.DeserializeXmlFile<PublishData>(Path.Combine(Data.AzureAppDir, Resources.PublishSettingsFileName));
                PublishData expectedPublishSettings = General.DeserializeXmlFile<PublishData>(publishSettingsFile);

                // Assert
                AzureAssert.AreEqualGlobalSettingsManager(new GlobalPathInfo(Data.AzureAppDir), expectedPublishSettings, globalSettingsManager);
                
                // Clean
                globalSettingsManager.DeleteGlobalSettingsManager();
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerLoadIgnoresPublishExisting()
        {
            var publishSettingsFile = Data.ValidPublishSettings.First();
            var subscriptionDataFile = Data.ValidSubscriptionsData.First();
            var outputSubscriptionDataFile = Path.Combine(Directory.GetParent(subscriptionDataFile).FullName, "outputNoPublish.xml");
            File.Copy(subscriptionDataFile, outputSubscriptionDataFile);

            // Create with both an existing ouput subscription data file and the publish settings file
            GlobalSettingsManager globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(Data.AzureAppDir, outputSubscriptionDataFile, publishSettingsFile);
            Assert.AreEqual(6, globalSettingsManager.Subscriptions.Count);

            // Remove one of the subscriptions from the publish settings file
            globalSettingsManager.Subscriptions.Remove("TestSubscription1");
            globalSettingsManager.SaveSubscriptions();

            // Load and make sure the subscription is still gone although it still is in the publish settings file
            globalSettingsManager = GlobalSettingsManager.Load(Data.AzureAppDir, outputSubscriptionDataFile);
            Assert.AreEqual(5, globalSettingsManager.Subscriptions.Count);

            // Clean
            globalSettingsManager.DeleteGlobalSettingsManager();
        }

        [TestMethod]
        public void GlobalSettingsManagerCreateNew()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                // Prepare
                GlobalSettingsManager globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(Data.AzureAppDir, null, fileName);
                PublishData expectedPublishSettings = General.DeserializeXmlFile<PublishData>(fileName);

                // Assert
                AzureAssert.AreEqualGlobalSettingsManager(new GlobalPathInfo(Data.AzureAppDir), expectedPublishSettings, globalSettingsManager);

                // Clean
                globalSettingsManager.DeleteGlobalSettingsManager();
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerCreateNewEmptyAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidSubscriptionsData)
            {
                try
                {
                    GlobalSettingsManager.Load(string.Empty, fileName);
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is ArgumentException);
                    Assert.AreEqual<string>(ex.Message, string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.AzureDirectoryName));
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerCreateNewNullAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidSubscriptionsData)
            {
                try
                {
                    GlobalSettingsManager.Load(null, fileName);
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is ArgumentException);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerCreateNewInvalidAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                foreach (string invalidDirectoryName in Data.InvalidServiceRootName)
                {
                    try
                    {
                        GlobalSettingsManager.Load(invalidDirectoryName, fileName);
                        Assert.Fail("No exception thrown");
                    }
                    catch (Exception ex)
                    {
                        Assert.IsTrue(ex is ArgumentException);
                        Assert.AreEqual<string>(ex.Message, "Illegal characters in path.");
                        Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                    }
                }
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerCreateNewInvalidPublishSettingsSchemaFail()
        {
            foreach (string fileName in Data.InvalidPublishSettings)
            {
                try
                {
                    GlobalSettingsManager.CreateFromPublishSettings(Data.AzureAppDir, null, fileName);
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is InvalidOperationException);
                    Assert.AreEqual<string>(ex.Message, string.Format(Resources.InvalidPublishSettingsSchema, fileName));
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerLoadExistingEmptyAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                try
                {
                    GlobalSettingsManager.Load("fake");
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is FileNotFoundException);
                    Assert.AreEqual<string>(ex.Message, Resources.GlobalSettingsManager_Load_PublishSettingsNotFound);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerLoadExistingNullAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                try
                {
                    GlobalSettingsManager.Load(null);
                    Assert.Fail("No exception thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is ArgumentException);
                    Assert.AreEqual<string>("Value cannot be null. Parameter name: 'azurePath'", ex.Message);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                }
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerLoadExistingInvalidDirectoryNameAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                foreach (string invalidDirectoryName in Data.InvalidServiceRootName)
                {
                    try
                    {
                        GlobalSettingsManager.Load(invalidDirectoryName);
                        Assert.Fail("No exception thrown");
                    }
                    catch (Exception ex)
                    {
                        Assert.IsTrue(ex is ArgumentException);
                        Assert.AreEqual<string>(ex.Message, "Illegal characters in path.");
                        Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                    }
                }
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerLoadDoesNotExistAzureDirectoryFail()
        {
            foreach (string fileName in Data.ValidPublishSettings)
            {
                foreach (string invalidDirectoryName in Data.InvalidServiceRootName)
                {
                    try
                    {
                        GlobalSettingsManager.Load("DoesNotExistDirectory");
                        Assert.Fail("No exception thrown");
                    }
                    catch (Exception ex)
                    {
                        Assert.IsTrue(ex is FileNotFoundException);
                        Assert.AreEqual<string>(ex.Message, Resources.GlobalSettingsManager_Load_PublishSettingsNotFound);
                        Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                    }
                }
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerCreateNewEmptyPublishSettingsFileFail()
        {
            try
            {
                GlobalSettingsManager.CreateFromPublishSettings(Data.AzureAppDir, null, string.Empty);
                Assert.Fail("No exception thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException);
                Assert.AreEqual<string>(ex.Message, string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.PublishSettings));
                Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerCreateNewNullPublishSettingsFileFail()
        {
            try
            {
                GlobalSettingsManager.CreateFromPublishSettings(Data.AzureAppDir, null, null);
                Assert.Fail("No exception thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException);
                Assert.AreEqual<string>(ex.Message, string.Format(Resources.InvalidOrEmptyArgumentMessage, Resources.PublishSettings));
                Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerCreateNewInvalidPublishSettingsFileFail()
        {
            foreach (string invalidFileName in Data.InvalidFileName)
            {
                Action<ArgumentException> verification = ex =>
                {
                    Assert.AreEqual<string>(ex.Message, Resources.IllegalPath);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                };

                Testing.AssertThrows<ArgumentException>(() => GlobalSettingsManager.CreateFromPublishSettings(Data.AzureAppDir, null, invalidFileName), verification);
            }
        }

        [TestMethod]
        public void GlobalSettingsManagerLoadInvalidPublishSettingsSchemaFail()
        {
            Testing.AssertThrows<FileNotFoundException>(
                () => GlobalSettingsManager.Load("DoesNotExistDirectory"),
                ex =>
                {
                    Assert.AreEqual<string>(ex.Message, Resources.GlobalSettingsManager_Load_PublishSettingsNotFound);
                    Assert.IsFalse(Directory.Exists(Data.AzureAppDir));
                });
        }

        [TestMethod]
        public void GetsTheAvailableEnvironments()
        {
            // Test
            List<WindowsAzureEnvironment> actual = GlobalSettingsManager.Instance.GetEnvironments();

            // Assert
            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(EnvironmentName.AzureCloud, actual[0].Name);
            Assert.AreEqual(EnvironmentName.AzureChinaCloud, actual[1].Name);
            Assert.AreEqual(WindowsAzureEnvironmentConstants.AzurePublishSettingsFileUrl, actual[0].PublishSettingsFileUrl);
            Assert.AreEqual(WindowsAzureEnvironmentConstants.ChinaPublishSettingsFileUrl, actual[1].PublishSettingsFileUrl);
            Assert.AreEqual(WindowsAzureEnvironmentConstants.AzureServiceEndpoint, actual[0].ServiceEndpoint);
            Assert.AreEqual(WindowsAzureEnvironmentConstants.ChinaServiceEndpoint, actual[1].ServiceEndpoint);
        }

        [TestMethod]
        public void GetPublishSettingsFileUrlUsingDefaultEnvironment()
        {
            // Setup
            string expected = WindowsAzureEnvironmentConstants.AzurePublishSettingsFileUrl;

            // Test
            string actual = GlobalSettingsManager.Instance.GetPublishSettingsFile();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPublishSettingsFileUrlUsingRealm()
        {
            // Setup
            string realmValue = "microsoft.com";
            StringBuilder expected = new StringBuilder(WindowsAzureEnvironmentConstants.AzurePublishSettingsFileUrl);
            expected.AppendFormat(Resources.RealmFormat, realmValue);
            
            // Test
            string actual = GlobalSettingsManager.Instance.GetPublishSettingsFile(realm: realmValue);

            // Assert
            Assert.AreEqual(expected.ToString(), actual);
        }

        [TestMethod]
        public void GetPublishSettingsFileUrlUsingNonExistingEnvironmentFail()
        {
            Testing.AssertThrows<KeyNotFoundException>(() => GlobalSettingsManager.Instance.GetPublishSettingsFile("no"));
        }

        [TestMethod]
        public void GetPublishSettingsFileUrlUsingSpecifiedEnvironmentAndRealm()
        {
            // Setup
            string realmValue = "microsoft.com";
            StringBuilder expected = new StringBuilder(WindowsAzureEnvironmentConstants.ChinaPublishSettingsFileUrl);
            expected.AppendFormat(Resources.RealmFormat, realmValue);

            // Test
            string actual = GlobalSettingsManager.Instance.GetPublishSettingsFile(EnvironmentName.AzureChinaCloud, realmValue);

            // Assert
            Assert.AreEqual(expected.ToString(), actual);
        }

        [TestMethod]
        public void GetPublishSettingsFileUrlIgnoreCase()
        {
            // Setup
            string realmValue = "microsoft.com";
            StringBuilder expected = new StringBuilder(WindowsAzureEnvironmentConstants.ChinaPublishSettingsFileUrl);
            expected.AppendFormat(Resources.RealmFormat, realmValue);

            // Test
            string actual = GlobalSettingsManager.Instance.GetPublishSettingsFile(
                EnvironmentName.AzureChinaCloud.ToLower(),
                realmValue);

            // Assert
            Assert.AreEqual(expected.ToString(), actual);
        }
    }
}