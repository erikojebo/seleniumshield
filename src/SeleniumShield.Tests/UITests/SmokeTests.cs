using System;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using SeleniumShield.Driver;

namespace SeleniumShield.Tests.UITests
{
    [TestFixture]
    public class SmokeTests
    {
        private SeleniumShieldDriver _driver;
        private FirefoxDriver _firefoxDriver;

        [SetUp]
        public void SetUp()
        {
            _firefoxDriver = new FirefoxDriver();

            var options = new SeleniumShieldDriverOptions()
            {
                BaseUrl = "http://localhost/seleniumshield",
                DefaultRetryDelayInSeconds = 0.1,
                DefaultTimeoutInSeconds = 5,
                SleepTimeBetweenActionsInMilliseconds = 0,
                MaxRetryCountPerCheckpoint = 3
            };

            _driver = new SeleniumShieldDriver(_firefoxDriver, options);
        }

        [Test]
        public void Restoring_checkpoint_replays_actions_taken_after_checkpoint()
        {
            int attemptNumber = 0;

            _driver.GoToRelativeUrl("/");

            _driver.SetCheckpoint(_driver.Refresh);
            _driver.WriteTo("#username", "kalle");
            _driver.WriteTo("#password", "p@ssw0rd");
            _driver.WithUnsafeDriver(d => attemptNumber++);
            _driver.WithUnsafeDriver(d =>
            {
                if (attemptNumber > 2)
                    return;

                _driver.WriteTo("#username", $"about to fail, #{attemptNumber}");

                Thread.Sleep(1000);

                throw new Exception("Configured to fail");
            });

            _driver.Submit(By.Id("value_submitter"));
        }

        [TearDown]
        public void TearDown()
        {
            _driver.Close();
            _driver.Quit();
        }
    }
}