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
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.DeploymentEntities;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.WebEntities;
    using Microsoft.WindowsAzure.Management.Websites;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SetAzureWebsiteTests : WebsitesTestBase
    {
        [TestMethod]
        public void SetAzureWebsiteProcess()
        {
            const string websiteName = "website1";
            const string webspaceName = "webspace";
            const string suffix = "azurewebsites.com";

            // Setup
            Mock<IWebsitesClient> clientMock = new Mock<IWebsitesClient>();
            clientMock.Setup(f => f.GetWebsiteDnsSuffix()).Returns(suffix);
            bool updatedSite = false;
            bool updatedSiteConfig = false;
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();

            Site site = new Site {Name = websiteName, WebSpace = webspaceName};
            SiteConfig siteConfig = new SiteConfig { NumberOfWorkers = 1};
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = webspaceName } });
            channel.GetSitesThunk = ar => new Sites(new List<Site> { site });
            channel.GetSiteThunk = ar => site;
            channel.GetSiteConfigThunk = ar => siteConfig;
            channel.UpdateSiteConfigThunk = ar =>
            {
                Assert.AreEqual(webspaceName, ar.Values["webspaceName"]);
                SiteConfig website = ar.Values["siteConfig"] as SiteConfig;
                Assert.IsNotNull(website);
                Assert.AreEqual(website.NumberOfWorkers, 3);
                siteConfig.NumberOfWorkers = website.NumberOfWorkers;
                updatedSiteConfig = true;
            };

            channel.UpdateSiteThunk = ar =>
            {
                Assert.AreEqual(webspaceName, ar.Values["webspaceName"]);
                Site website = ar.Values["site"] as Site;
                Assert.IsNotNull(website);
                Assert.AreEqual(websiteName, website.Name);
                Assert.IsTrue(website.HostNames.Any(hostname => hostname.Equals(string.Format("{0}.{1}", websiteName, suffix))));
                Assert.IsNotNull(website.HostNames.Any(hostname => hostname.Equals("stuff.com")));
                site.HostNames = website.HostNames;
                updatedSite = true;
            };

            // Test
            SetAzureWebsiteCommand setAzureWebsiteCommand = new SetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionId },
                NumberOfWorkers = 3,
                WebsitesClient = clientMock.Object
            };

            setAzureWebsiteCommand.ExecuteCmdlet();
            Assert.IsTrue(updatedSiteConfig);
            Assert.IsFalse(updatedSite);

            // Test updating site only and not configurations
            updatedSite = false;
            updatedSiteConfig = false;
            setAzureWebsiteCommand = new SetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionId },
                HostNames = new [] { "stuff.com" },
                WebsitesClient = clientMock.Object
            };

            setAzureWebsiteCommand.ExecuteCmdlet();
            Assert.IsFalse(updatedSiteConfig);
            Assert.IsTrue(updatedSite);
        }
    }
}
