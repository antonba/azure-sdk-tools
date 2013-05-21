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

namespace Microsoft.WindowsAzure.Management.Utilities.Common
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class HttpClientExtensions
    {
        private static void AddUserAgent(HttpClient client)
        {
            if (!client.DefaultRequestHeaders.UserAgent.Contains(ApiConstants.UserAgentValue))
            {
                client.DefaultRequestHeaders.UserAgent.Add(ApiConstants.UserAgentValue);
            }
        }

        private static void LogResponse(
            string statusCode,
            HttpResponseHeaders headers,
            string content,
            Action<string> Logger)
        {
            if (Logger != null)
            {
                Logger(General.GetHttpResponseLog(statusCode, headers, content));
            }
        }

        private static void LogRequest(
            string method,
            string requestUri,
            HttpRequestHeaders headers,
            string body,
            Action<string> Logger)
        {
            if (Logger != null)
            {
                Logger(General.GetHttpRequestLog(method, requestUri, headers, body));
            }
        }

        private static T GetFormat<T>(
            HttpClient client,
            string requestUri,
            Action<string> Logger,
            Func<string, string> formatter,
            Func<string, T> serializer)
        {
            AddUserAgent(client);
            LogRequest(
                HttpMethod.Get.Method,
                client.BaseAddress + requestUri,
                client.DefaultRequestHeaders,
                string.Empty,
                Logger);
            HttpResponseMessage response = client.GetAsync(requestUri).Result;
            string content = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
            LogResponse(response.StatusCode.ToString(), response.Headers, formatter(content), Logger);

            return serializer(content);
        }

        private static string GetRawBody(
            HttpClient client,
            string requestUri,
            Action<string> Logger,
            Func<string, string> formatter)
        {
            AddUserAgent(client);
            LogRequest(
                HttpMethod.Get.Method,
                client.BaseAddress + requestUri,
                client.DefaultRequestHeaders,
                string.Empty,
                Logger);
            HttpResponseMessage response = client.GetAsync(requestUri).Result;
            string content = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
            LogResponse(response.StatusCode.ToString(), response.Headers, formatter(content), Logger);

            return content;
        }

        public static T GetJson<T>(this HttpClient client, string requestUri, Action<string> Logger)
        {
            return GetFormat<T>(client, requestUri, Logger, General.TryFormatJson, JsonConvert.DeserializeObject<T>);
        }

        public static string GetXml(this HttpClient client, string requestUri, Action<string> Logger)
        {
            return GetRawBody(client, requestUri, Logger, General.FormatXml);
        }

        public static T GetXml<T>(this HttpClient client, string requestUri, Action<string> Logger)
        {
            return GetFormat<T>(client, requestUri, Logger, General.FormatXml, General.DeserializeXmlString<T>);
        }

        public static HttpResponseMessage PostAsJsonAsync(
            this HttpClient client,
            string requestUri,
            JObject json,
            Action<string> Logger)
        {
            AddUserAgent(client);

            LogRequest(
                HttpMethod.Post.Method,
                client.BaseAddress + requestUri,
                client.DefaultRequestHeaders,
                JsonConvert.SerializeObject(json, Formatting.Indented),
                Logger);
            HttpResponseMessage response = client.PostAsJsonAsync(requestUri, json).Result;
            string content = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
            LogResponse(
                response.StatusCode.ToString(),
                response.Headers,
                General.TryFormatJson(content),
                Logger);

            return response;
        }
    }
}
