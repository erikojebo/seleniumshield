using System;
using NUnit.Framework;
using SeleniumShield.Driver.Exceptions;
using SeleniumShield.Driver.Infrastructure;
using SeleniumShield.Tests.Infrastructure;

namespace SeleniumShield.Tests.TaskRunners
{
    [TestFixture]
    public class RetryingTaskRunnerTests
    {
        [Test]
        public void Retrying_with_()
        {
            var executionCount = 0;

            try
            {
                RetryingTaskRunner.ExecuteWithRetryAttemptLimit(() =>
                {
                    executionCount++;
                    throw new PreconfiguredFailureException();
                }, 3);
            }
            catch (SeleniumShieldDriverException)
            {
            }

            Assert.AreEqual(4, executionCount);
        }
    }
}