﻿// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.Storage.Test.Common.Cmdlet
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Storage.Common;
    using Microsoft.WindowsAzure.Management.Storage.Common.Cmdlet;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Storage.Auth;

    [TestClass]
    public class NewAzureStorageContextTest : StorageTestBase
    {
        /// <summary>
        /// StorageCmdletBase command
        /// </summary>
        public NewAzureStorageContext command = null;

        [TestInitialize]
        public void InitCommand()
        {
            command = new NewAzureStorageContext
            {
                CommandRuntime = new MockCommandRuntime()
            };
        }

        [TestCleanup]
        public void CleanCommand()
        {
            command = null;
        }

        [TestMethod]
        public void GetStorageAccountByNameAndKeyTest()
        {
            AssertThrows<FormatException>(()=>command.GetStorageAccountByNameAndKey("a", "key", false));
            command.GetStorageAccountByNameAndKey("a", "Xg+4nFQ832QfisuH/CkQwdQUmlqrZebQTJWpAQZ6klWjTVsIBVZy5xNdCDje4EWP0gdWK8vIFAX8LOmz85Wmcg==", false);
        }

        [TestMethod]
        public void GetStorageAccountBySasTokenTest()
        {
            command.GetStorageAccountBySasToken("a", "?st=d", true);
            AssertThrows<Exception>(()=>command.GetStorageAccountBySasToken("a", string.Empty, false));
            AssertThrows<Exception>(() => command.GetStorageAccountBySasToken("a", "token", false));
        }

        [TestMethod]
        public void GetStorageAccountByConnectionStringTest()
        {
            AssertThrows<Exception>(() => command.GetStorageAccountByConnectionString(String.Empty));
            AssertThrows<Exception>(() => command.GetStorageAccountByConnectionString("connection string"));

            Assert.IsNotNull(command.GetStorageAccountByConnectionString("UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://myProxyUri"));
        }

        [TestMethod]
        public void GetLocalDevelopmentStorageAccountTest()
        {
            Assert.IsNotNull(command.GetLocalDevelopmentStorageAccount());
        }

        [TestMethod]
        public void GetAnonymousStorageAccountTest()
        {
            Assert.IsNotNull(command.GetAnonymousStorageAccount("a", false));
        }

        [TestMethod]
        public void GetStorageAccountWithEndPointTest()
        {
            string name = string.Empty;
            StorageCredentials credential = new StorageCredentials();
            AssertThrows<ArgumentException>(() => command.GetStorageAccountWithEndPoint(credential, name, false), String.Format(Resources.ObjectCannotBeNull, StorageNouns.StorageAccountName));

            name = "test";
            Assert.IsNotNull(command.GetStorageAccountWithEndPoint(credential, name, false));
        }

        [TestMethod]
        public void ExecuteNewAzureStorageContextCmdlet()
        {
            AssertThrows<ArgumentException>(() => command.ExecuteCmdlet(), Resources.DefaultStorageCredentialsNotFound);
        }

        [TestMethod]
        public void GetDefaultEndPointDomainTest()
        {
            Assert.AreEqual(command.GetDefaultEndPointDomain(), Resources.DefaultStorageEndPointDomain);
        }

        [TestMethod]
        public void GetStorageDomainFromEndPointTest()
        {
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://myaccount.blob.core.windows.net/"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://myaccount.table.core.windows.net/"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://myaccount.queue.core.windows.net/"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://blob.core.windows.net/"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://table.core.windows.net/"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://queue.core.windows.net/"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://core.windows.net"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("core.windows.net"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("  core.windows.net   "), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("core.windows.net/"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("core.abc"), "abc");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("abc.blob.core.chinacloudapi.cn"), "chinacloudapi.cn");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("https://abc.blob.core.chinacloudapi.cn/container/blob"), "chinacloudapi.cn");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("https://abc.blob.core.chinacloudapi.cn:8010/container/blob"), "chinacloudapi.cn:8010");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("abc.blob.core.chinacloudapi.cn:8010"), "chinacloudapi.cn:8010");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("abc.blob.core.chinacloudapi.cn:8010/contianer/abc.txt"), "chinacloudapi.cn:8010");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://core.chinacloudapi.cn"), "chinacloudapi.cn");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://abccore.blob.core.windows.net"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("http://abccore.blob.cOrE.windOWS.Net"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("abccore.blob.core.windows.net"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("ftp://abccore.blob.core.windows.net"), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("ftp://abccore.blob.core.windows.net  "), "windows.net");
            Assert.AreEqual(command.GetStorageDomainFromEndPoint("unknown://abccore.blob.core.windows.net  "), "windows.net");
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint("http://www.bing.com"));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint("windows.net"));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint(""));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint("core."));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint(@"C:\windows\core.abc\etc"));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint(@"C:/windows/core.abc/etc"));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint(@"file:///core.abc/etc"));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint(@"ftp://bing.com"));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint(@"unknown://bing.com"));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint(null));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint("http://127.0.0.1"));
            AssertThrows<ArgumentException>(() => command.GetStorageDomainFromEndPoint("http://127.0.0.1/account/table"));
        }
    }
}
