using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumShield.Exceptions;

namespace SeleniumShield.Extensions
{
    public static class WebDriverExtensions
    {
        public static IWebElement TryFindElement(this IWebDriver driver, By by)
        {
            try
            {
                return driver.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        public static void GoToUrl(this IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);
        }

        public static IWebElement FindBySelector(this IWebDriver driver, string cssSelector, int timeoutInSeconds = 30)
        {
            return FindElement(driver, By.CssSelector(cssSelector), timeoutInSeconds);
        }

        public static IEnumerable<IWebElement> FindAllBySelector(this IWebDriver driver, string cssSelector)
        {
            return driver.FindElements(By.CssSelector(cssSelector));
        }

        public static IWebElement FindElement(this IWebDriver driver, By @by, IWebElement parent, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => parent.FindElement(by));
            }

            return parent.FindElement(by);
        }

        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }

            return driver.FindElement(by);
        }

        public static void Click(this IWebDriver driver, string selector, int timeoutInSeconds = 30)
        {
            driver.ExecuteWithRetry(() =>
            {
                var element = driver.FindBySelector(selector, timeoutInSeconds);
                element.Click();
            }, timeoutInSeconds);
        }

        public static IWebElement WaitUntilElementIsVisible(this IWebDriver driver, string selector, int timeoutInSeconds)
        {
            return driver.WaitUntil(By.CssSelector(selector), drv => drv.GetIfVisible(By.CssSelector(selector)), timeoutInSeconds);
        }

        public static IWebElement WaitUntilElementIsEnabled(this IWebDriver driver, string selector, int timeoutInSeconds)
        {
            return driver.WaitUntil(By.CssSelector(selector), drv => drv.GetIfEnabled(selector), timeoutInSeconds);
        }

        public static IWebElement WaitUntil(this IWebDriver driver, By by, Func<IWebDriver, IWebElement> func, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(func);
            }

            return driver.FindElement(by);
        }

        public static void WaitUntilWindowOpens(this IWebDriver driver, int timeoutInSeconds, int expectedWindowCount = 2)
        {
            var startTime = DateTime.Now;

            while (driver.WindowHandles.Count < expectedWindowCount && (DateTime.Now - startTime).TotalSeconds < timeoutInSeconds)
            {
                Thread.Sleep(200);
            }
        }

        public static bool IsVisible(this IWebDriver driver, string selector)
        {
            var elements = driver.FindElements(By.CssSelector(selector));
            return elements.Any() && elements.All(x => x.Displayed);
        }

        public static IWebElement GetIfVisible(this IWebDriver driver, By by)
        {
            var element = driver.FindElement(by);
            return element != null && element.Displayed ? element : null;
        }

        public static IWebElement GetIfEnabled(this IWebDriver driver, string selector)
        {
            try
            {
                var element = driver.FindElement(By.CssSelector(selector));
                return element != null && element.Enabled ? element : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        }

        public static void WriteTo(this IWebDriver driver, string cssSelector, DateTime date, int timeoutInSeconds = 30, bool includeTime = false)
        {
            var dateString = date.ToShortDateString();
            var timeString = date.ToShortTimeString();

            var text = dateString;

            if (includeTime)
            {
                text += " " + timeString;
            }

            driver.WriteTo(cssSelector, text, timeoutInSeconds);
        }

        public static void WriteTo(this IWebDriver driver, string cssSelector, int number, int timeoutInSeconds)
        {
            driver.WriteTo(cssSelector, number.ToString(), timeoutInSeconds);
        }

        public static void Submit(this IWebDriver driver, string cssSelector)
        {
            driver.ExecuteWithRetry(() => driver.FindBySelector(cssSelector, 10).Submit());
        }

        public static void WriteTo(this IWebDriver driver, string cssSelector, string text, int timeoutInSeconds = 30, bool clear = true)
        {
            driver.ExecuteWithRetry(() =>
            {
                var element = driver.FindBySelector(cssSelector, timeoutInSeconds: timeoutInSeconds);

                if (clear)
                {
                    element.Clear();
                }

                element.SendKeys(text);

                element = driver.FindBySelector(cssSelector, timeoutInSeconds: timeoutInSeconds);

                // Make sure the text was written successfully. Sometimes only part of the text gets written,
                // possibly due to focus issues. In that case, retry as long as we are within the timeout time.
                var actualText = element.GetAttribute("value");

                if (actualText != text)
                {
                    throw new SeleniumShieldException(
                        "Could not write '{0}' to element with selector '{1}'. Actual text: '{2}'", 
                        text, 
                        cssSelector, 
                        actualText);
                }
            }, timeoutInSeconds);
        }

        public static void ExecuteWithRetry(this IWebDriver driver, Action action, int timeoutInSeconds = 30)
        {
            driver.ExecuteWithRetry(action, DateTime.Now.AddSeconds(30));
        }

        public static void ExecuteWithRetry(this IWebDriver driver, Action action, DateTime retryDeadline)
        {
            driver.ExecuteWithRetry(action, retryDeadline, null);
        }

        private static void ExecuteWithRetry(this IWebDriver driver, Action action, DateTime retryDeadline, Exception lastException)
        {
            if (DateTime.Now > retryDeadline)
            {
                throw new SeleniumShieldException(
                    "Could not complete the requested action within the allowed timeout. See inner exception for details.", 
                    lastException);
            }

            try
            {
                action();
            }
            catch (Exception exception) // It doesn't really matter what happened, just retry until the timeout has passed
            {
                Thread.Sleep(1000);
                driver.ExecuteWithRetry(action, retryDeadline, exception);
            }
        }

        public static void AssertElementInnerHtml(this IWebDriver driver, string cssSelector, string expectedText)
        {
            var element = driver.FindBySelector(cssSelector);
            
            var actualContent = element.GetAttribute("innerHTML");

            if (actualContent != expectedText)
            {
                throw new SeleniumShieldAssertionFailedException(
                    "Assert failed: Expected element with selector '{0}' to have innerHTML '{1}', but actual value was: '{2}'",
                    cssSelector,
                    expectedText,
                    actualContent);
            }
        }
    }
}