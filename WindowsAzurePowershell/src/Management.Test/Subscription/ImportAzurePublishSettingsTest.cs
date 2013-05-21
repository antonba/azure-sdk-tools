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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Management.Utilities.Subscription.Contract;
    using Microsoft.WindowsAzure.Management.Subscription;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ImportAzurePublishSettingsTest
    {
        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void TestImportSubscriptionProcess()
        {
            MockCommandRuntime mockCommandRuntime;
            ImportAzurePublishSettingsCommand cmdlet;
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new ImportAzurePublishSettingsCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            var globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(GlobalPathInfo.GlobalSettingsDirectory, null, Data.ValidPublishSettings.First());

            
            cmdlet.ImportSubscriptionFile(
                Data.ValidPublishSettings.First(),
                null);

            var currentSubscription = cmdlet.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionName, Data.Subscription1);
            Assert.IsTrue(currentSubscription.IsDefault);

            globalSettingsManager.DeleteGlobalSettingsManager();
        }

        [TestMethod]
        public void TestImportSubscriptionPublishSettingsOnlyProcess()
        {
            MockCommandRuntime mockCommandRuntime;
            ImportAzurePublishSettingsCommand cmdlet;
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new ImportAzurePublishSettingsCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.ImportSubscriptionFile(
                Data.ValidPublishSettings.First(),
                null);

            var currentSubscription = cmdlet.GetCurrentSubscription();
            Assert.AreEqual(Data.Subscription1, currentSubscription.SubscriptionName);
            Assert.IsTrue(currentSubscription.IsDefault);
        }


        [TestMethod]
        public void TestImportSubscriptionPublishSettingsSecondVersionOnlyProcess()
        {
            MockCommandRuntime mockCommandRuntime;
            ImportAzurePublishSettingsCommand cmdlet;
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new ImportAzurePublishSettingsCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.SubscriptionClient = CreateMockSubscriptionClient();
            cmdlet.ImportSubscriptionFile(
                Data.ValidPublishSettings2.First(),
                null);

            var currentSubscription = cmdlet.GetCurrentSubscription();
            Assert.AreEqual(Data.SampleSubscription1, currentSubscription.SubscriptionName);
            Assert.AreEqual("https://newmanagement.core.windows.net/", currentSubscription.ServiceEndpoint);
            Assert.IsNotNull(currentSubscription.Certificate);
            Assert.IsTrue(currentSubscription.IsDefault);
        }

        [TestMethod]
        public void TestImportSubscriptionPublishSettingsOnlyMultipleTimesProcess()
        {
            MockCommandRuntime mockCommandRuntime;
            ImportAzurePublishSettingsCommand cmdlet;
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new ImportAzurePublishSettingsCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.SubscriptionClient = CreateMockSubscriptionClient();
            cmdlet.ImportSubscriptionFile(
                Data.ValidPublishSettings.First(),
                null);
            
            var subscriptions = cmdlet.GetSubscriptions(null);
            
            SubscriptionData currentSubscription = cmdlet.GetCurrentSubscription();
            Assert.AreEqual(Data.Subscription1, currentSubscription.SubscriptionName);
            Assert.IsTrue(currentSubscription.IsDefault);
            
            SubscriptionData newCurrentSubscription = subscriptions.Values.FirstOrDefault(s => !s.SubscriptionId.Equals(currentSubscription.SubscriptionId));
            cmdlet.SetCurrentSubscription(newCurrentSubscription);

            cmdlet.ImportSubscriptionFile(
                Data.ValidPublishSettings.First(),
                null);

            currentSubscription = cmdlet.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionId, newCurrentSubscription.SubscriptionId);
        }

        [TestMethod]
        public void TestImportPublishSettingsWithPassingDirectory()
        {
            MockCommandRuntime mockCommandRuntime;
            ImportAzurePublishSettingsCommand cmdlet;
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new ImportAzurePublishSettingsCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.SubscriptionClient = CreateMockSubscriptionClient();
            string directoryName = "testdir";
            string fileName = "myfile.publishsettings";
            string filePath = Path.Combine(directoryName, fileName);
            Directory.CreateDirectory(directoryName);
            File.WriteAllText(filePath, File.ReadAllText(Data.ValidPublishSettings.First()));
            cmdlet.PublishSettingsFile = directoryName;

            cmdlet.ExecuteCmdlet();

            SubscriptionData currentSubscription = cmdlet.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionName, Data.Subscription1);
            Assert.IsTrue(currentSubscription.IsDefault);
            Assert.AreEqual<string>(filePath, mockCommandRuntime.OutputPipeline[0].ToString());
        }

        [TestMethod]
        public void TestImportPublishSettingsWithoutPassingDirectory()
        {
            MockCommandRuntime mockCommandRuntime;
            ImportAzurePublishSettingsCommand cmdlet;
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new ImportAzurePublishSettingsCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.SubscriptionClient = CreateMockSubscriptionClient();
            string directoryName = "testdir";
            string fileName = "myfile.publishsettings";
            Directory.CreateDirectory(directoryName);
            string originalDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetFullPath(directoryName));
            File.WriteAllText(fileName, File.ReadAllText(Data.ValidPublishSettings.First()));

            cmdlet.ExecuteCmdlet();

            SubscriptionData currentSubscription = cmdlet.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionName, Data.Subscription1);
            Assert.IsTrue(currentSubscription.IsDefault);
            Assert.AreEqual<string>(Path.GetFullPath(fileName), mockCommandRuntime.OutputPipeline[0].ToString());
            Directory.SetCurrentDirectory(originalDirectory);
            Assert.AreEqual<string>(originalDirectory, Directory.GetCurrentDirectory());
        }

        [TestMethod]
        public void TestImportPublishSettingsWithNoPublishSettingsFilesFound()
        {
            MockCommandRuntime mockCommandRuntime;
            ImportAzurePublishSettingsCommand cmdlet;
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new ImportAzurePublishSettingsCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.SubscriptionClient = CreateMockSubscriptionClient();
            string directoryName = "testdir3";
            string originalDirectory = Directory.GetCurrentDirectory();
            Directory.CreateDirectory(directoryName);
            Directory.SetCurrentDirectory(Path.GetFullPath(directoryName));

            Testing.AssertThrows<Exception>(
                () => cmdlet.ExecuteCmdlet(), 
                string.Format(Resources.NoPublishSettingsFilesFoundMessage, Directory.GetCurrentDirectory()));
            Directory.SetCurrentDirectory(originalDirectory);
            Assert.AreEqual<string>(originalDirectory, Directory.GetCurrentDirectory());
        }

        [TestMethod]
        public void TestImportPublishSettingsWithMultiplePublishSettingsFilesFound()
        {
            MockCommandRuntime mockCommandRuntime;
            ImportAzurePublishSettingsCommand cmdlet;
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new ImportAzurePublishSettingsCommand();
            cmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.SubscriptionClient = CreateMockSubscriptionClient();
            string directoryName = "testdir2";
            string fileName1 = "myfile1.publishsettings";
            string fileName2 = "myfile2.publishsettings";
            string filePath1 = Path.Combine(directoryName, fileName1);
            string filePath2 = Path.Combine(directoryName, fileName2);
            Directory.CreateDirectory(directoryName);
            File.WriteAllText(filePath1, File.ReadAllText(Data.ValidPublishSettings.First()));
            File.WriteAllText(filePath2, File.ReadAllText(Data.ValidPublishSettings.First()));
            cmdlet.PublishSettingsFile = directoryName;

            cmdlet.ExecuteCmdlet();

            SubscriptionData currentSubscription = cmdlet.GetCurrentSubscription();
            Assert.AreEqual(currentSubscription.SubscriptionName, Data.Subscription1);
            Assert.IsTrue(currentSubscription.IsDefault);
            Assert.AreEqual<string>(filePath1, mockCommandRuntime.OutputPipeline[0].ToString());
            Assert.AreEqual<string>(string.Format(Resources.MultiplePublishSettingsFilesFoundMessage, filePath1), mockCommandRuntime.WarningStream[0]);
        }

        private ISubscriptionClient CreateMockSubscriptionClient()
        {
            var mock = new Mock<ISubscriptionClient>();

            mock.Setup(c => c.ListResourcesAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(() => Task.Factory.StartNew(() => new ProviderResource[0].AsEnumerable()));
            mock.Setup(c => c.RegisterResourceTypeAsync(It.IsAny<string>()))
                .Returns(() => Task.Factory.StartNew(() => true));

            return mock.Object;
        }
    }
}