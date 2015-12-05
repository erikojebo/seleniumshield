using OpenQA.Selenium.Firefox;
using SeleniumShield.Driver;

namespace SeleniumShield.Tests.UITests
{
    public class UITestBase
    {
        protected SeleniumShieldDriver Driver;
        protected SeleniumShieldDriverOptions Options;
        private FirefoxDriver _firefoxDriver;

        protected void CreateDriver()
        {
            _firefoxDriver = new FirefoxDriver();

            Options = new SeleniumShieldDriverOptions()
            {
                BaseUrl = "http://localhost/seleniumshield",
                DefaultRetryDelayInSeconds = 0.5,
                DefaultTimeoutInSeconds = 5,
                SleepTimeBetweenActionsInMilliseconds = 0,
                MaxRetryCountPerCheckpoint = 3
            };

            Driver = new SeleniumShieldDriver(_firefoxDriver, Options);
        }

        protected void CleanUp()
        {
            Driver.Close();
            Driver.Quit();
        }
    }
}