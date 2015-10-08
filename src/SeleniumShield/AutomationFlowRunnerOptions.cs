using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using SeleniumShield.Output;

namespace SeleniumShield
{
    public class AutomationFlowRunnerOptions
    {
        public AutomationFlowRunnerOptions(params Func<IWebDriver>[] driverFactoryFuncs)
        {
            WebDriverFactories = driverFactoryFuncs.ToList();

            if (!WebDriverFactories.Any())
            {
                WebDriverFactories.Add(() => new FirefoxDriver());
            }

            MaxAllowedStepFailCount = 3;

            ResultListener = new ConsoleListener();
        }

        public IList<Func<IWebDriver>> WebDriverFactories { get; set; }
        public int MaxAllowedStepFailCount { get; set; }
        public IResultListener ResultListener { get; set; }
    }
}