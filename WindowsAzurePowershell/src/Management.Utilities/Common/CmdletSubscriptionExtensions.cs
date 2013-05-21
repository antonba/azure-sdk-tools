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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;

    public static class CmdletSubscriptionExtensions
    {
        public static ISessionManager SessionManager = new PsSessionManager();

        public static SubscriptionData GetCurrentSubscription(this PSCmdlet cmdlet)
        {
            // Check if there is a current subscription already set
            var currentSubscription = SessionManager.GetVariable(cmdlet, ManagementConstants.CurrentSubscriptionEnvironmentVariable) as SubscriptionData;
            if (currentSubscription == null)
            {
                try
                {
                    // Check if there is a default subscription available
                    GlobalSettingsManager globalSettingsManager = GlobalSettingsManager.Load(GlobalPathInfo.GlobalSettingsDirectory);
                    currentSubscription =
                        globalSettingsManager.Subscriptions.Values.FirstOrDefault(subscription => subscription.IsDefault);

                    if (currentSubscription != null)
                    {
                        // Set the default subscription to be the new current subscription
                        SessionManager.SetVariable(cmdlet, ManagementConstants.CurrentSubscriptionEnvironmentVariable,
                                                   currentSubscription);
                    }
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }

            return currentSubscription;
        }

        public static void SetCurrentSubscription(this PSCmdlet cmdlet, string subscriptionName, string subscriptionDataFile)
        {
            if (subscriptionName == null)
            {
                throw new ArgumentNullException("subscriptionName", Resources.InvalidSubscriptionName);
            }

            var globalSettingsManager = GlobalSettingsManager.Load(GlobalPathInfo.GlobalSettingsDirectory, subscriptionDataFile);

            SubscriptionData subscriptionData;
            if (!globalSettingsManager.Subscriptions.TryGetValue(subscriptionName, out subscriptionData))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.InvalidSubscription,
                    subscriptionName));
            }

            SetCurrentSubscription(cmdlet, subscriptionData);
        }

        public static void SetCurrentSubscription(this PSCmdlet cmdlet, SubscriptionData subscription)
        {
            SessionManager.SetVariable(cmdlet, ManagementConstants.CurrentSubscriptionEnvironmentVariable, subscription);
        }

        public static void ClearCurrentSubscription(this PSCmdlet cmdlet)
        {
            SessionManager.ClearVariable(cmdlet, ManagementConstants.CurrentSubscriptionEnvironmentVariable);
        }

        public static SubscriptionData GetSubscription(this PSCmdlet cmdlet, string subscriptionName, string subscriptionDataFile)
        {
            if (subscriptionName == null)
            {
                throw new ArgumentNullException("subscriptionName", Resources.InvalidSubscriptionName);
            }

            IDictionary<string, SubscriptionData> subscriptions = GetSubscriptions(cmdlet, subscriptionDataFile);

            SubscriptionData subscriptionData;

            if (subscriptions.TryGetValue(subscriptionName, out subscriptionData))
            {
                return subscriptionData;
            }

            return null;
        }

        public static IDictionary<string, SubscriptionData> GetSubscriptions(this PSCmdlet cmdlet, string subscriptionDataFile)
        {
            return GlobalSettingsManager.Load(GlobalPathInfo.GlobalSettingsDirectory, subscriptionDataFile).Subscriptions;
        }

        public static void SetDefaultSubscription(this PSCmdlet cmdlet, string subscriptionName, string subscriptionDataFile)
        {
            GlobalSettingsManager globalSettingsManager = GlobalSettingsManager.Load(GlobalPathInfo.GlobalSettingsDirectory, subscriptionDataFile);
            if (globalSettingsManager.Subscriptions.Count > 1)
            {
                var defaultSubscription = globalSettingsManager.Subscriptions.Values.FirstOrDefault(subscription => subscription.IsDefault);
                if (defaultSubscription != null)
                {
                    defaultSubscription.IsDefault = false;
                }

                if (subscriptionName != null)
                {
                    defaultSubscription = globalSettingsManager.Subscriptions.Values.First(subscription => subscription.SubscriptionName == subscriptionName);
                    defaultSubscription.IsDefault = true;
                }
                else
                {
                    defaultSubscription = globalSettingsManager.Subscriptions.Values.First();
                    defaultSubscription.IsDefault = true;
                }

                globalSettingsManager.SaveSubscriptions();
            }
        }

        public static void UpdateSubscriptions(this PSCmdlet cmdlet, IDictionary<string, SubscriptionData> subscriptionsData, string subscriptionDataFile)
        {
            GlobalSettingsManager globalSettingsManager = GlobalSettingsManager.Load(GlobalPathInfo.GlobalSettingsDirectory, subscriptionDataFile);
            globalSettingsManager.SubscriptionManager.Subscriptions = subscriptionsData;
            globalSettingsManager.SaveSubscriptions();
        }
    }
}