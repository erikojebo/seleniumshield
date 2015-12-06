using NUnit.Framework;
using SeleniumShield.Driver.Exceptions;
using SeleniumShield.Driver.Infrastructure;
using SeleniumShield.Tests.Infrastructure;

namespace SeleniumShield.Tests.UITests
{
    [TestFixture]
    public class RetryTests : UITestBase
    {
        [SetUp]
        public void SetUp()
        {
            CreateDriver();
        }

        [Test]
        public void Executing_block_with_max_allowed_retry_attemps_retries_on_failure()
        {
            var executionCount = 0;

            try
            {
                Driver.WithRetryAttemptLimit(() =>
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

        [Test]
        public void Retry_blocks_with_max_allowed_retry_attemps_can_be_nested()
        {
            var outerExecutionCount = 0;
            var innerExecutionCount = 0;

            try
            {
                Driver.WithRetryAttemptLimit(() =>
                {
                    outerExecutionCount++;

                    Driver.WithRetryAttemptLimit(() =>
                    {
                        innerExecutionCount++;
                        throw new PreconfiguredFailureException();
                    }, 4);

                    throw new PreconfiguredFailureException();
                }, 2);
            }
            catch (SeleniumShieldDriverException)
            {
            }

            Assert.AreEqual(3, outerExecutionCount);
            Assert.AreEqual(3 * 5, innerExecutionCount);
        }

        [TearDown]
        public void TearDown()
        {
            CleanUp();
        }
    }
}