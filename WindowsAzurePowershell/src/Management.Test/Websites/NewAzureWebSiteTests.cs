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
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.WebEntities;
    using Microsoft.WindowsAzure.Management.Websites;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NewAzureWebsiteTests : WebsitesTestBase
    {
        [TestMethod]
        public void ProcessNewWebsiteTest()
        {
            const string websiteName = "website1";
            const string webspaceName = "webspace1";
            const string suffix = "azurewebsites.com";

            // Setup
            Mock<IWebsitesClient> clientMock = new Mock<IWebsitesClient>();
            clientMock.Setup(f => f.GetWebsiteDnsSuffix()).Returns(suffix);
            bool created = true;
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace>
            {
                new WebSpace { Name = "webspace1", GeoRegion = "webspace1" },
                new WebSpace { Name = "webspace2", GeoRegion = "webspace2" }
            });

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

            channel.CreateSiteThunk = ar =>
                                          {
                                              Assert.AreEqual(webspaceName, ar.Values["webspaceName"]);
                                              Site website = ar.Values["site"] as Site;
                                              Assert.IsNotNull(website);
                                              Assert.AreEqual(websiteName, website.Name);
                                              Assert.IsNotNull(website.HostNames.FirstOrDefault(hostname => hostname.Equals(string.Format("{0}.{1}", websiteName, suffix))));
                                              created = true;
                                              return website;
                                          };

            // Test
            MockCommandRuntime mockRuntime = new MockCommandRuntime();
            NewAzureWebsiteCommand newAzureWebsiteCommand = new NewAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = mockRuntime,
                Name = websiteName,
                Location = webspaceName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionId },
                WebsitesClient = clientMock.Object
            };

            newAzureWebsiteCommand.ExecuteCmdlet();
            Assert.IsTrue(created);
            Assert.AreEqual<string>(websiteName, (mockRuntime.OutputPipeline[0] as SiteWithConfig).Name);
        }

        [TestMethod]
        public void GetsWebsiteDefaultLocation()
        {
            const string websiteName = "website1";
            const string suffix = "azurewebsites.com";

            // Setup
            Mock<IWebsitesClient> clientMock = new Mock<IWebsitesClient>();
            clientMock.Setup(f => f.GetWebsiteDnsSuffix()).Returns(suffix);
            bool created = true;
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces();

            channel.GetSiteConfigThunk = ar =>
            {
                return new SiteConfig
                {
                    PublishingUsername = "user1"
                };
            };

            channel.CreateSiteThunk = ar =>
            {
                Site website = ar.Values["site"] as Site;
                Assert.IsNotNull(website);
                Assert.AreEqual(websiteName, website.Name);
                Assert.IsNotNull(website.HostNames.FirstOrDefault(hostname => hostname.Equals(string.Format("{0}.{1}", websiteName, suffix))));
                created = true;
                return website;
            };

            // Test
            MockCommandRuntime mockRuntime = new MockCommandRuntime();
            NewAzureWebsiteCommand newAzureWebsiteCommand = new NewAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = mockRuntime,
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionId },
                WebsitesClient = clientMock.Object
            };

            newAzureWebsiteCommand.ExecuteCmdlet();
            Assert.IsTrue(created);
            Assert.AreEqual<string>(websiteName, (mockRuntime.OutputPipeline[0] as SiteWithConfig).Name);
            clientMock.Verify(f => f.GetDefaultLocation(), Times.Once());
        }
    }
}
