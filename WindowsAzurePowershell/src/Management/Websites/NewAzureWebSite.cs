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

namespace Microsoft.WindowsAzure.Management.Websites
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.Text.RegularExpressions;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;
    using Microsoft.WindowsAzure.Management.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.Github;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.WebEntities;
    using GitClass = Microsoft.WindowsAzure.Management.Utilities.Websites.Services.Git;

    /// <summary>
    /// Creates a new azure website.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureWebsite"), OutputType(typeof(SiteWithConfig))]
    public class NewAzureWebsiteCommand : WebsiteContextBaseCmdlet, IGithubCmdlet
    {
        public IWebsitesClient WebsitesClient { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The geographic region to create the website.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Custom host name to use.")]
        [ValidateNotNullOrEmpty]
        public string Hostname
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The publishing user name.")]
        [ValidateNotNullOrEmpty]
        public string PublishingUsername
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Configure git on the web site and local folder.")]
        public SwitchParameter Git
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Configure github on the web site.")]
        public SwitchParameter GitHub
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The github credentials.")]
        [ValidateNotNullOrEmpty]
        public PSCredential GithubCredentials
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The github repository.")]
        [ValidateNotNullOrEmpty]
        public string GithubRepository
        {
            get;
            set;
        }

        public IGithubServiceManagement GithubChannel { get; set; }

        /// <summary>
        /// Initializes a new instance of the NewAzureWebsiteCommand class.
        /// </summary>
        public NewAzureWebsiteCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureWebsiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public NewAzureWebsiteCommand(IWebsitesServiceManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureWebsiteCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        /// <param name="githubChannel">
        /// Channel used for communication with the github APIs.
        /// </param>
        public NewAzureWebsiteCommand(IWebsitesServiceManagement channel, IGithubServiceManagement githubChannel)
        {
            Channel = channel;
            GithubChannel = githubChannel;
        }

        internal void CopyIisNodeWhenServerJsPresent()
        {
            if (!File.Exists("iisnode.yml") && (File.Exists("server.js") || File.Exists("app.js")))
            {
                string cmdletPath = Directory.GetParent(MyInvocation.MyCommand.Module.Path).FullName;
                File.Copy(Path.Combine(cmdletPath, "Scaffolding/Node/Website/iisnode.yml"), "iisnode.yml");
            }
        }

        internal void UpdateLocalConfigWithSiteName(string websiteName, string webspace)
        {
            GitWebsite gitWebsite = new GitWebsite(websiteName, webspace);
            gitWebsite.WriteConfiguration();
        }

        internal string GetPublishingUser()
        {
            if (!string.IsNullOrEmpty(PublishingUsername))
            {
                return PublishingUsername;
            }

            // Get publishing users
            IList<string> users = null;
            try
            {
                InvokeInOperationContext(() => { users = RetryCall(s => Channel.GetSubscriptionPublishingUsers(s)); });
            }
            catch
            {
                throw new Exception(Resources.NeedPublishingUsernames);
            }

            IEnumerable<string> validUsers = users.Where(user => !string.IsNullOrEmpty(user)).ToList();
            if (!validUsers.Any())
            {
                if (ShouldProcess(Resources.InvalidGitCredentials) && ShouldContinue("", ""))
                {
                    General.LaunchWindowsAzurePortal(null, null);
                }
            } 
            
            if (!(validUsers.Count() == 1 && users.Count() == 1))
            {
                throw new Exception(Resources.MultiplePublishingUsernames);
            }

            return users.First();
        }

        internal void InitializeRemoteRepo(string webspace, string websiteName)
        {
            try
            {
                // Create website repository
                InvokeInOperationContext(() => RetryCall(s => Channel.CreateSiteRepository(s, webspace, websiteName)));
            }
            catch (Exception ex)
            {
                // Handle site creating indepently so that cmdlet is idempotent.
                string message = ProcessException(ex, false);
                if (message.Equals(string.Format(Resources.WebsiteRepositoryAlreadyExists,
                                                 Name)))
                {
                    WriteWarning(message);
                }
                else
                {
                    WriteExceptionError(new Exception(message));
                }
            }
        }

        internal void AddRemoteToLocalGitRepo(Site website)
        {
            // Get remote repos
            IList<string> remoteRepositories = GitClass.GetRemoteRepositories();
            if (remoteRepositories.Any(repository => repository.Equals("azure")))
            {
                // Removing existing azure remote alias
                GitClass.RemoveRemoteRepository("azure");
            }

            string repositoryUri = website.GetProperty("RepositoryUri");

            string uri = GitClass.GetUri(repositoryUri, Name, PublishingUsername);
            GitClass.AddRemoteRepository("azure", uri);
        }

        [EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        public override void ExecuteCmdlet()
        {
            WebsitesClient = WebsitesClient ?? new WebsitesClient(CurrentSubscription, WriteDebug);
            string suffix = WebsitesClient.GetWebsiteDnsSuffix();

            if (Git && GitHub)
            {
                throw new Exception("Please run the command with either -Git or -GitHub options. Not both.");
            }

            if (Git)
            {
                PublishingUsername = GetPublishingUser();
            }

            WebSpaces webspaceList = null;
            InvokeInOperationContext(() => { webspaceList = RetryCall(s => Channel.GetWebSpacesWithCache(s)); });
            if (Git && webspaceList.Count == 0)
            {
                // If location is still empty or null, give portal instructions.
                string error = string.Format(Resources.PortalInstructions, Name);
                throw new Exception(!Git
                    ? error
                    : string.Format("{0}\n{1}", error, Resources.PortalInstructionsGit));
            }

            WebSpace webspace = null;
            if (string.IsNullOrEmpty(Location))
            {
                // If no location was provided as a parameter, try to default it
                webspace = webspaceList.FirstOrDefault();
                if (webspace == null)
                {
                    webspace = new WebSpace
                    {
                        GeoRegion = WebsitesClient.GetDefaultLocation(),
                        Subscription = CurrentSubscription.SubscriptionId,
                        Plan = "VirtualDedicatedPlan"
                    };
                }
            }
            else
            {
                // Find the webspace that corresponds to the georegion
                webspace = webspaceList.FirstOrDefault(w => w.GeoRegion.Equals(Location, StringComparison.OrdinalIgnoreCase));
                if (webspace == null)
                {
                    // If no webspace corresponding to the georegion was found, attempt to create it
                    webspace = new WebSpace
                    {
                        Name = Regex.Replace(Location.ToLower(), " ", "") + "webspace",
                        GeoRegion = Location,
                        Subscription = CurrentSubscription.SubscriptionId,
                        Plan = "VirtualDedicatedPlan"
                    };
                }
            }

            SiteWithWebSpace website = new SiteWithWebSpace
            {
                Name = Name,
                HostNames = new[] { string.Format("{0}.{1}", Name, suffix) },
                WebSpace = webspace.Name,
                WebSpaceToCreate = webspace
            };

            if (!string.IsNullOrEmpty(Hostname))
            {
                List<string> newHostNames = new List<string>(website.HostNames);
                newHostNames.Add(Hostname);
                website.HostNames = newHostNames.ToArray();
            }

            try
            {
                InvokeInOperationContext(() => RetryCall(s => Channel.CreateSite(s, webspace.Name, website)));

                // If operation succeeded try to update cache with new webspace if that's the case
                if (webspaceList.FirstOrDefault(ws => ws.Name.Equals(webspace.Name)) == null)
                {
                    Cache.AddWebSpace(CurrentSubscription.SubscriptionId, webspace);
                }

                Cache.AddSite(CurrentSubscription.SubscriptionId, website);
                SiteConfig websiteConfiguration = null;
                InvokeInOperationContext(() => 
                {
                    websiteConfiguration = RetryCall(s => Channel.GetSiteConfig(s, website.WebSpace, website.Name));
                    WaitForOperation(CommandRuntime.ToString());
                });
                WriteObject(new SiteWithConfig(website, websiteConfiguration));
            }
            catch (ProtocolException ex)
            {
                // Handle site creating indepently so that cmdlet is idempotent.
                string message = ProcessException(ex, false);
                if (message.Equals(string.Format(Resources.WebsiteAlreadyExistsReplacement,
                                                 Name)) && (Git || GitHub))
                {
                    WriteWarning(message);
                }
                else if (message.Equals(Resources.DefaultHostnamesValidation))
                {
                    WriteExceptionError(new Exception(Resources.InvalidHostnameValidation));
                }
                else
                {
                    WriteExceptionError(new Exception(message));
                }
            }

            if (Git || GitHub)
            {
                try
                {
                    Directory.SetCurrentDirectory(SessionState.Path.CurrentFileSystemLocation.Path);
                }
                catch (Exception)
                {
                    // Do nothing if session state is not present
                }

                LinkedRevisionControl linkedRevisionControl = null;
                if (Git)
                {
                    linkedRevisionControl = new GitClient(this);
                }
                else if (GitHub)
                {
                    linkedRevisionControl = new GithubClient(this, GithubCredentials, GithubRepository);
                }

                linkedRevisionControl.Init();
 
                CopyIisNodeWhenServerJsPresent();
                UpdateLocalConfigWithSiteName(Name, webspace.Name);

                InitializeRemoteRepo(webspace.Name, Name);

                Site updatedWebsite = RetryCall(s => Channel.GetSite(s, webspace.Name, Name, "repositoryuri,publishingpassword,publishingusername"));
                if (Git)
                {
                    AddRemoteToLocalGitRepo(updatedWebsite);
                }

                linkedRevisionControl.Deploy(updatedWebsite);
                linkedRevisionControl.Dispose();
            }
        }

        public Action<string> GetLogger()
        {
            return WriteDebug;
        }
    }
}
