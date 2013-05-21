// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.Utilities.Websites
{
    using System;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.DeploymentEntities;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.WebEntities;

    public interface IWebsitesClient
    {
        /// <summary>
        /// Starts log streaming for the given website.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="path">The log path, by default root</param>
        /// <param name="message">The substring message</param>
        /// <param name="endStreaming">Predicate to end streaming</param>
        /// <param name="waitInternal">The fetch wait interval</param>
        /// <returns>The log line</returns>
        IEnumerable<string> StartLogStreaming(
            string name,
            string path,
            string message,
            Predicate<string> endStreaming,
            int waitInternal);

        /// <summary>
        /// List log paths for a given website.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        List<LogPath> ListLogPaths(string name);

        /// <summary>
        /// Gets the application diagnostics settings
        /// </summary>
        /// <param name="name">The website name</param>
        /// <returns>The website application diagnostics settings</returns>
        DiagnosticsSettings GetApplicationDiagnosticsSettings(string name);

        /// <summary>
        /// Restarts a website.
        /// </summary>
        /// <param name="name">The website name</param>
        void RestartAzureWebsite(string name);

        /// <summary>
        /// Starts a website.
        /// </summary>
        /// <param name="name">The website name</param>
        void StartAzureWebsite(string name);

        /// <summary>
        /// Stops a website.
        /// </summary>
        /// <param name="name">The website name</param>
        void StopAzureWebsite(string name);

        /// <summary>
        /// Gets a website instance.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <returns>The website instance</returns>
        Site GetWebsite(string name);
                
        /// <summary>
        /// Gets the website configuration.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <returns>The website configuration object</returns>
        SiteWithConfig GetWebsiteConfiguration(string name);

        /// <summary>
        /// Enables site diagnostic.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="webServerLogging">Flag for webServerLogging</param>
        /// <param name="detailedErrorMessages">Flag for detailedErrorMessages</param>
        /// <param name="failedRequestTracing">Flag for failedRequestTracing</param>
        void EnableSiteDiagnostic(
            string name,
            bool webServerLogging,
            bool detailedErrorMessages,
            bool failedRequestTracing);

        /// <summary>
        /// Disables site diagnostic.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="webServerLogging">Flag for webServerLogging</param>
        /// <param name="detailedErrorMessages">Flag for detailedErrorMessages</param>
        /// <param name="failedRequestTracing">Flag for failedRequestTracing</param>
        void DisableSiteDiagnostic(
            string name,
            bool webServerLogging,
            bool detailedErrorMessages,
            bool failedRequestTracing);

        /// <summary>
        /// Enables application diagnostic.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="output">The application log output, FileSystem or StorageTable</param>
        /// <param name="logLevel">The log level</param>
        /// <param name="properties">The diagnostic setting properties</param>
        void EnableApplicationDiagnostic(
            string name,
            WebsiteDiagnosticOutput output,
            Dictionary<DiagnosticProperties, object> properties);

        /// <summary>
        /// Disables application diagnostic.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="output">The application log output, FileSystem or StorageTable</param>
        void DisableApplicationDiagnostic(string name, WebsiteDiagnosticOutput output);

        /// <summary>
        /// Sets an AppSetting of a website.
        /// </summary>
        /// <param name="name">The website name</param>
        /// <param name="key">The app setting name</param>
        /// <param name="value">The app setting value</param>
        void AddAppSetting(string name, string key, string value);

        /// <summary>
        /// Lists available website locations.
        /// </summary>
        /// <returns>List of location names</returns>
        List<string> ListAvailableLocations();

        /// <summary>
        /// Gets the default website DNS suffix for the current environment.
        /// </summary>
        /// <returns>The website DNS suffix</returns>
        string GetWebsiteDnsSuffix();

        /// <summary>
        /// Gets the default location for websites.
        /// </summary>
        /// <returns>The default location name.</returns>
        string GetDefaultLocation();
    }

    public enum WebsiteState
    {
        Running,
        Stopped
    }

    public enum WebsiteDiagnosticType
    {
        Site,
        Application
    }

    public enum WebsiteDiagnosticOutput
    {
        FileSystem,
        StorageTable
    }

    public enum DiagnosticProperties
    {
        StorageAccountName,
        LogLevel
    }
}
