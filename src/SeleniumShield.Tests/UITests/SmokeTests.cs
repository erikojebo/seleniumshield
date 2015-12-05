using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;

namespace SeleniumShield.Tests.UITests
{
    [TestFixture]
    public class SmokeTests : UITestBase
    {
        [SetUp]
        public void SetUp()
        {
            CreateDriver();

            Options.SleepTimeBetweenActionsInMilliseconds = 1000;
        }

        [Test]
        public void Restoring_checkpoint_replays_actions_taken_after_checkpoint()
        {
            int attemptNumber = 0;

            Driver.GoToRelativeUrl("/");

            Driver.SetCheckpoint(Driver.Refresh);
            Driver.WriteTo("#username", "kalle");
            Driver.WriteTo("#password", "p@ssw0rd");
            Driver.WithUnsafeDriver(d => attemptNumber++);
            Driver.WithUnsafeDriver(d =>
            {
                if (attemptNumber > 2)
                    return;

                Driver.WriteTo("#username", $"about to fail, #{attemptNumber}");

                throw new Exception("Configured to fail");
            });

            Driver.Submit(By.Id("value_submitter"));

            Driver.WaitUntilElementIsVisible(By.Id("container1"));

            Assert.AreEqual(3, attemptNumber);
        }

        [TearDown]
        public void TearDown()
        {
            CleanUp();
        }
    }
}