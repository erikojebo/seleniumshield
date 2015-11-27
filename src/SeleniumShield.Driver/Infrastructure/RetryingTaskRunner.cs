using System;
using System.Threading;
using OpenQA.Selenium;
using SeleniumShield.Driver.Exceptions;

namespace SeleniumShield.Driver.Infrastructure
{
    internal class RetryingTaskRunner
    {
        public static void ExecuteWithRetry(Action action, int timeoutInMilliseconds, int retryDelainInMilliseconds = 1000, bool throwOnTimeOut = true)
        {
            var retryDeadline = DateTime.Now.AddMilliseconds(timeoutInMilliseconds);
            ExecuteWithRetry(action, retryDeadline, retryDelainInMilliseconds, throwOnTimeOut);
        }

        public static void ExecuteWithRetry(Action action, DateTime retryDeadline, int retryDelainInMilliseconds, bool throwOnTimeOut = true)
        {
            Exception lastException = null;

            while (DateTime.Now <= retryDeadline)
            {
                try
                {
                    action();

                    // Everything worked, just return
                    return;
                }
                catch (Exception exception) // It doesn't really matter what happened, just retry until the timeout has passed
                {
                    lastException = exception;
                    Thread.Sleep(retryDelainInMilliseconds);
                }
            }

            // The deadline was not met, and we have not yet returned from the method, so it failed
            if (throwOnTimeOut)
            {
                throw new SeleniumShieldDriverTimeoutException(
                    "Could not complete the requested action within the allowed timeout. See inner exception for details.",
                    lastException);
            }
        }
    }
}