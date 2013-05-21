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
    using System.Management.Automation;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using ServiceManagement;
    using Properties;

    public abstract class ServiceManagementBaseCmdlet : CloudBaseCmdlet<IServiceManagement>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposing the client would also dispose the channel we are returning.")]
        protected override IServiceManagement CreateChannel()
        {
            // If ShareChannel is set by a unit test, use the same channel that
            // was passed into out constructor.  This allows the test to submit
            // a mock that we use for all network calls.
            if (ShareChannel)
            {
                return Channel;
            }

            var messageInspectors = new List<IClientMessageInspector>
            {
                new ServiceManagementClientOutputMessageInspector(),
                new HttpRestMessageInspector(this.WriteDebug)
            };

            var clientOptions = new ServiceManagementClientOptions(null, null, null, 0, RetryPolicy.NoRetryPolicy, ServiceManagementClientOptions.DefaultOptions.WaitTimeForOperationToComplete, messageInspectors);
            var smClient = new ServiceManagementClient(new Uri(this.ServiceEndpoint), CurrentSubscription.SubscriptionId, CurrentSubscription.Certificate, clientOptions);

            Type serviceManagementClientType = typeof(ServiceManagementClient);
            PropertyInfo propertyInfo = serviceManagementClientType.GetProperty("SyncService", BindingFlags.Instance | BindingFlags.NonPublic);
            var syncService = (IServiceManagement)propertyInfo.GetValue(smClient, null);

            return syncService;
        }

        /// <summary>
        /// Invoke the given operation within an OperationContextScope if the
        /// channel supports it.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        protected override void InvokeInOperationContext(Action action)
        {
            IContextChannel contextChannel = ToContextChannel();
            if (contextChannel != null)
            {
                using (new OperationContextScope(contextChannel))
                {
                    action();
                }
            }
            else
            {
                action();
            }
        }

        protected virtual IContextChannel ToContextChannel()
        {
            try
            {
                return Channel.ToContextChannel();
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void ExecuteClientAction(object input, string operationDescription, Action<string> action)
        {
            Operation operation = null;

            WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionBeginOperation, operationDescription));

            RetryCall(action);
            operation = GetOperation();

            WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionCompletedOperation, operationDescription));

            if (operation != null)
            {
                var context = new ManagementOperationContext
                {
                    OperationDescription = operationDescription,
                    OperationId = operation.OperationTrackingId,
                    OperationStatus = operation.Status
                };

                WriteObject(context, true);
            }
        }

        protected void ExecuteClientActionInOCS(object input, string operationDescription, Action<string> action)
        {
            IContextChannel contextChannel = null;

            try
            {
                contextChannel = Channel.ToContextChannel();
            }
            catch (Exception)
            {
                // Do nothing, proceed.
            }

            if (contextChannel != null)
            {
                object context = null;

                using (new OperationContextScope(contextChannel))
                {
                    Operation operation = null;

                    WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSBeginOperation, operationDescription));

                    try
                    {
                        RetryCall(action);
                        operation = GetOperation();
                    }
                    catch (ServiceManagementClientException ex)
                    {
                        WriteErrorDetails(ex);
                    }

                    WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSCompletedOperation, operationDescription));

                    if (operation != null)
                    {
                        context = new ManagementOperationContext
                        {
                            OperationDescription = operationDescription,
                            OperationId = operation.OperationTrackingId,
                            OperationStatus = operation.Status
                        };
                    }
                }

                if (context != null)
                {
                    WriteObject(context, true);
                }
            }
            else
            {
                RetryCall(action);
            }
        }

        protected virtual void WriteErrorDetails(ServiceManagementClientException exception)
        {
            if (CommandRuntime != null)
            {
                WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
            }
        }

        protected void ExecuteClientActionInOCS<TResult>(object input, string operationDescription, Func<string, TResult> action, Func<Operation, TResult, object> contextFactory) where TResult : class
        {
            IContextChannel contextChannel = null;

            try
            {
                contextChannel = Channel.ToContextChannel();
            }
            catch (Exception)
            {
                // Do nothing, proceed.
            }

            if (contextChannel != null)
            {
                object context = null;

                using (new OperationContextScope(contextChannel))
                {
                    TResult result = null;
                    Operation operation = null;

                    WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSBeginOperation, operationDescription));

                    try
                    {
                        result = RetryCall(action);
                        operation = GetOperation();
                    }
                    catch (ServiceManagementClientException ex)
                    {
                        WriteErrorDetails(ex);
                    }

                    WriteVerboseWithTimestamp(string.Format(Resources.ServiceManagementExecuteClientActionInOCSCompletedOperation, operationDescription));

                    if (result != null && operation != null)
                    {
                        context = contextFactory(operation, result);
                    }
                }

                if (context != null)
                {
                    WriteObject(context, true);
                }
            }
            else
            {
                TResult result = RetryCall(action);
                if (result != null)
                {
                    WriteObject(result, true);
                }
            }
        }

        protected Operation GetOperation()
        {
            Operation operation = null;

            try
            {
                string operationId = RetrieveOperationId();

                if (!string.IsNullOrEmpty(operationId))
                {
                    operation = RetryCall(s => GetOperationStatus(this.CurrentSubscription.SubscriptionId, operationId));

                    if (string.Compare(operation.Status, OperationState.Failed, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var errorMessage = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", operation.Status, operation.Error.Message);
                        var exception = new Exception(errorMessage);
                        WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
                    }
                }
                else
                {
                    operation = new Operation
                    {
                        OperationTrackingId = string.Empty,
                        Status = OperationState.Failed
                    };
                }
            }
            catch (ServiceManagementClientException ex)
            {
                WriteErrorDetails(ex);
            }

            return operation;
        }

        protected override Operation GetOperationStatus(string subscriptionId, string operationId)
        {
            var channel = (IServiceManagement)Channel;
            return channel.GetOperationStatus(subscriptionId, operationId);
        }
    }
}