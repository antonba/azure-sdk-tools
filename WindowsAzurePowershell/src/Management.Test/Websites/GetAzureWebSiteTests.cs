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

namespace Microsoft.WindowsAzure.Management.Test.Websites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.WindowsAzure.Management.Subscription;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using Microsoft.WindowsAzure.Management.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.WebEntities;
    using Microsoft.WindowsAzure.Management.Websites;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetAzureWebsiteTests : WebsitesTestBase
    {
        [TestMethod]
        public void ProcessGetWebsiteTest()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSitesThunk = ar =>
                                           {
                                               if (ar.Values["webspaceName"].Equals("webspace1"))
                                               {
                                                   return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1" }});
                                               }

                                               return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2" } });
                                           };

            // Test
            GetAzureWebsiteCommand getAzureWebsiteCommand = new GetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionId }
            };

            getAzureWebsiteCommand.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).OutputPipeline.Count);
            var sites = (IEnumerable<Site>)((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.IsNotNull(sites);
            Assert.IsTrue(sites.Any(website => (website).Name.Equals("website1") && (website).WebSpace.Equals("webspace1")));
            Assert.IsTrue(sites.Any(website => (website).Name.Equals("website2") && (website).WebSpace.Equals("webspace2")));
        }

        [TestMethod]
        public void GetWebsiteProcessShowTest()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSiteThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Site { Name = "website1", WebSpace = "webspace1" };
                }

                return new Site { Name = "website2", WebSpace = "webspace2" };
            };

            channel.GetSiteConfigThunk = ar =>
            {
                if (ar.Values["name"].Equals("website1") && ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new SiteConfig
                    {
                        PublishingUsername = "user1"
                    };
                }

                return null;
            };

            channel.GetSitesThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1" } });
                }

                return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2" } });
            };

            // Test
            GetAzureWebsiteCommand getAzureWebsiteCommand = new GetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionId },
                Name = "website1",
                WebsitesClient = new Mock<IWebsitesClient>().Object
            };

            getAzureWebsiteCommand.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).OutputPipeline.Count);

            var website = ((MockCommandRuntime) getAzureWebsiteCommand.CommandRuntime).OutputPipeline[0] as SiteWithConfig;
            Assert.IsNotNull(website);
            Assert.IsNotNull(website);
            Assert.AreEqual("website1", website.Name);
            Assert.AreEqual("webspace1", website.WebSpace);
            Assert.AreEqual("user1", website.PublishingUsername);

            // Run with mixed casing
            getAzureWebsiteCommand = new GetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new SubscriptionData { SubscriptionId = "GetAzureWebSiteTests_GetWebsiteProcessShowTest" },
                Name = "WEBSiTe1",
                WebsitesClient = new Mock<IWebsitesClient>().Object
            };

            getAzureWebsiteCommand.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).OutputPipeline.Count);

            website = ((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).OutputPipeline[0] as SiteWithConfig;
            Assert.IsNotNull(website);
            Assert.IsNotNull(website);
            Assert.AreEqual("website1", website.Name);
            Assert.AreEqual("webspace1", website.WebSpace);
            Assert.AreEqual("user1", website.PublishingUsername);
        }

        [TestMethod]
        public void ProcessGetWebsiteWithNullSubscription()
        {
            // Setup
            GlobalSettingsManager globalSettingsManager = GlobalSettingsManager.CreateFromPublishSettings(
                GlobalPathInfo.GlobalSettingsDirectory,
                null,
                Data.ValidPublishSettings[0]);
            RemoveAzureSubscriptionCommand removeCmdlet = new RemoveAzureSubscriptionCommand();
            removeCmdlet.CommandRuntime = new MockCommandRuntime();
            ICollection<string> subscriptions = globalSettingsManager.Subscriptions.Keys;

            foreach (string subscription in subscriptions)
            {
                removeCmdlet.RemoveSubscriptionProcess(subscription ,null);
            }

            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSitesThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1" } });
                }

                return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2" } });
            };

            // Test
            GetAzureWebsiteCommand getAzureWebsiteCommand = new GetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = null
            };

            Testing.AssertThrows<Exception>(() => getAzureWebsiteCommand.ExecuteCmdlet(), Resources.NoDefaultSubscriptionMessage);
        }

        [TestMethod]
        public void TestGetAzreWebsiteWithDiagnosticsSettings()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSiteThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Site { Name = "website1", WebSpace = "webspace1", State = "Running" };
                }

                return new Site { Name = "website2", WebSpace = "webspace2" };
            };

            channel.GetSiteConfigThunk = ar =>
            {
                if (ar.Values["name"].Equals("website1") && ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new SiteConfig
                    {
                        PublishingUsername = "user1"
                    };
                }

                return null;
            };

            channel.GetSitesThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1", State = "Running" } });
                }

                return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2", State = "Running" } });
            };

            Mock<IWebsitesClient> websitesClientMock = new Mock<IWebsitesClient>();
            GetAzureWebsiteCommand getAzureWebsiteCommand = new GetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionId },
                Name = "website1",
                WebsitesClient = websitesClientMock.Object
            };

            // Test
            getAzureWebsiteCommand.ExecuteCmdlet();

            // Assert
            Assert.AreEqual(1, ((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).OutputPipeline.Count);
            websitesClientMock.Verify(f => f.GetApplicationDiagnosticsSettings("website1"), Times.Once());
        }
    }
}
