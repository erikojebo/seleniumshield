using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumShield.Driver.Checkpointing;
using SeleniumShield.Driver.Exceptions;
using SeleniumShield.Driver.Infrastructure;
using SeleniumShield.Driver.Logging;

namespace SeleniumShield.Driver
{
    public class SeleniumShieldDriver
    {
        private readonly IWebDriver _driverToShield;
        private readonly SeleniumShieldDriverOptions _options;
        private readonly ActionRunner _actionRunner = new ActionRunner();

        public SeleniumShieldDriver(IWebDriver driverToShield, SeleniumShieldDriverOptions options)
        {
            _driverToShield = driverToShield;
            _options = options;
        }

        public void Dispose()
        {
            _driverToShield.Dispose();
        }

        public void Close()
        {
            _driverToShield.Close();
        }

        public void Quit()
        {
            _driverToShield.Quit();
        }

        public void Refresh()
        {
            _driverToShield.Navigate().Refresh();
        }

        public void GoToRelativeUrl(string relativeUrl)
        {
            var absoluteUrl = UrlBuilder.ToAbsoluteUrl(_options.BaseUrl, relativeUrl);
            _driverToShield.Navigate().GoToUrl(absoluteUrl);
        }

        public void GoToUrl(string url)
        {
            _driverToShield.Navigate().GoToUrl(url);
        }

        public string Url
        {
            get { return _driverToShield.Url; }
            set { _driverToShield.Url = value; }
        }

        public string Title => _driverToShield.Title;
        public string PageSource => _driverToShield.PageSource;
        public string CurrentWindowHandle => _driverToShield.CurrentWindowHandle;
        public ReadOnlyCollection<string> WindowHandles => _driverToShield.WindowHandles;

        public string GetText(By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            return WithUnsafeDriver(driver =>
            {
                var element = driver.FindElement(by);
                return element.Text;
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public string GetAttribute(string attributeName, By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            return WithUnsafeDriver(driver =>
            {
                var element = driver.FindElement(by);
                return element.GetAttribute(attributeName);
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void MoveTo(string selector, double? timeoutInSeconds = null)
        {
            MoveTo(By.CssSelector(selector), timeoutInSeconds);
        }

        public void MoveTo(By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            WithUnsafeDriver(driver =>
            {
                var element = driver.FindElement(by);
                new Actions(driver).MoveToElement(element).Perform();
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void MoveTo(IWebElement element, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            WithUnsafeDriver(driver =>
            {
                new Actions(driver).MoveToElement(element).Perform();
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void ScrollTo(string cssSelector, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            WithUnsafeDriver(drv =>
            {
                ExecuteJavaScript($"document.querySelector('{cssSelector}').scrollIntoView()");

                var isElementInViewportReturnValue = ExecuteJavaScript($@"return (function () {{
    var element = document.querySelector('{cssSelector}');
    var rect = element.getBoundingClientRect();

    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) && 
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
}})();");
                var isElementInViewport = (bool)isElementInViewportReturnValue;

                if (!isElementInViewport)
                {
                    throw new SeleniumShieldDriverException($"Could not scroll to element with selector: '{cssSelector}'");
                }
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void WriteTo(
            string cssSelector,
            string text,
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null,
            bool clear = true)
        {
            WriteTo(By.CssSelector(cssSelector), text, timeoutInSeconds, retryDelayInSeconds, clear);
        }

        public void WriteTo(
            By by,
            string text, 
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null,
            bool clear = true)
        {
            WithRetry(() =>
            {
                MoveTo(by);

                var element = FindElement(by, timeoutInSeconds, retryDelayInSeconds);

                if (clear)
                {
                    element.Clear();
                }

                element.SendKeys(text);

                element = FindElement(by, timeoutInSeconds, retryDelayInSeconds);

                // Make sure the text was written successfully. Sometimes only part of the text gets written,
                // possibly due to focus issues. In that case, retry as long as we are within the timeout time.
                var actualText = element.GetAttribute("value");

                if (actualText != text)
                {
                    throw new SeleniumShieldDriverException($"Could not write '{text}' to element. Actual text: '{actualText}'");
                }
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void Submit(string selector, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            Submit(selector, timeoutInSeconds, retryDelayInSeconds, By.CssSelector(selector));
        }

        private void Submit(string selector, double? timeoutInSeconds, double? retryDelayInSeconds, By @by)
        {
            WithRetry(() =>
            {
                MoveTo(selector);

                var element = FindElement(@by, timeoutInSeconds, retryDelayInSeconds);
                element.Submit();
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void Click(string selector, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            Click(By.CssSelector(selector), timeoutInSeconds, retryDelayInSeconds);
        }

        public void Click(By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            var subTimeoutInSeconds = GetSubTimeoutInSeconds(timeoutInSeconds, 2);

            WithRetry(() =>
            {
                MoveTo(by, subTimeoutInSeconds, retryDelayInSeconds);

                var element = FindElement(by, subTimeoutInSeconds, retryDelayInSeconds);
                element.Click();

            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public object ExecuteJavaScript(string script)
        {
            return WithUnsafeDriver(driver =>
            {
                var javaScriptExecutor = driver as IJavaScriptExecutor;

                if (javaScriptExecutor == null)
                    throw new SeleniumShieldDriverException(
                        "The current web driver implementation ('{0}') does not support executing JavaScript",
                        _driverToShield.GetType().Name);

                return ((IJavaScriptExecutor)driver).ExecuteScript(script);
            });
        }

        public void WaitUntilElementIsVisible(
            By by, 
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null)
        {
            WaitUntil(() =>
            {
                var element = _driverToShield.FindElement(by);

                if (element == null)
                {
                    LogWarning("WaitUntilElementIsVisible", "Could not find element");
                }
                else if (!element.Displayed)
                {
                    LogWarning("WaitUntilElementIsVisible", "Element was found, but not displayed");
                }

                return element != null && element.Displayed;
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void WaitUntilElementIsRemovedOrHidden(
            By by,
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null)
        {
            WaitUntil(() =>
            {
                var element = TryFindElementUnsafe(by);

                if (element != null && element.Displayed)
                {
                    LogWarning("WaitUntilElementIsRemovedOrHidden", "Element was found, and is still displayed");
                }

                return element == null || !element.Displayed;
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void WaitUntilElementIsEnabled(
            By by,
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null)
        {
            WaitUntil(() =>
            {
                var element = _driverToShield.FindElement(by);

                if (element == null)
                {
                    LogWarning("WaitUntilElementIsEnabled", "Could not find element");
                }
                else if (!element.Enabled)
                {
                    LogWarning("WaitUntilElementIsEnabled", "Element was found, but not enabled");
                }

                return element != null && element.Enabled;
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void WaitUntilElementIsDisabled(
            By by,
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null)
        {
            WaitUntil(() =>
            {
                var element = _driverToShield.FindElement(by);

                if (element == null)
                {
                    LogWarning("WaitUntilElementIsDisabled", "Could not find element");
                }
                else if (!element.Enabled)
                {
                    LogWarning("WaitUntilElementIsDisabled", "Element was found, but not disabled");
                }

                return element != null && !element.Enabled;
            }, timeoutInSeconds, retryDelayInSeconds);
        }


        private void LogWarning(string method, string message)
        {
            _options.Logger?.Log(LogLevel.Warning, $"{method}: {message}");
        }

        public void WaitUntilWindowOpens(
            int expectedWindowCount,
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null)
        {
            WaitUntil(
                () => _driverToShield.WindowHandles.Count >= expectedWindowCount, 
                timeoutInSeconds, 
                retryDelayInSeconds);
        }

        public void WaitUntil(
            Func<bool> func, 
            double? timeoutInSeconds = null, 
            double? retryDelayInSeconds = null)
        {
            WithRetry(() =>
            {
                var wasSuccessful = func();

                if (!wasSuccessful)
                    throw new SeleniumShieldDriverException("Condition was not met");
            });
        }

        public bool IsReadOnly(By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            var subTimeoutInSeconds = GetSubTimeoutInSeconds(timeoutInSeconds, 2);

            return WithRetry(() =>
            {
                var element = FindElement(by, subTimeoutInSeconds, retryDelayInSeconds);
                return element.GetAttribute("readonly") != null;
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public bool IsEnabled(By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            var subTimeoutInSeconds = GetSubTimeoutInSeconds(timeoutInSeconds, 2);

            return WithRetry(() =>
            {
                var element = FindElement(by, subTimeoutInSeconds, retryDelayInSeconds);
                return element.Enabled;
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public bool Exists(By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            return WithRetry(() => TryFindElementUnsafe(by) != null, timeoutInSeconds, retryDelayInSeconds);
        }

        public bool IsVisible(By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            var subTimeoutInSeconds = GetSubTimeoutInSeconds(timeoutInSeconds, 2);

            return WithRetry(() =>
            {
                var element = FindElement(by, subTimeoutInSeconds, retryDelayInSeconds);
                return element.Displayed;
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void SelectByText(
            string selectElementSelector, 
            string optionTextToSelect, 
            double? timeoutInSeconds = null, 
            double? retryDelayInSeconds = null)
        {
            SelectByText(
                By.CssSelector(selectElementSelector), 
                optionTextToSelect, 
                timeoutInSeconds, 
                retryDelayInSeconds);
        }

        public void SelectByText(
            By by,
            string optionTextToSelect,
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null)
        {
            WithRetry(() =>
            {
                MoveTo(by);

                var selectElement = FindElement(by, timeoutInSeconds, retryDelayInSeconds);

                var wrappedSelectElement = new SelectElement(selectElement);

                var optionToSelect = wrappedSelectElement.Options.First(x => x.Text.Trim() == optionTextToSelect);

                MoveTo(by);
                MoveTo(optionToSelect, timeoutInSeconds, retryDelayInSeconds);

                wrappedSelectElement.SelectByValue(optionToSelect.GetAttribute("value"));

            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void SelectByValue(
            string selectElementSelector,
            string optionValueToSelect,
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null)
        {
            SelectByValue(By.CssSelector(selectElementSelector), optionValueToSelect, timeoutInSeconds, retryDelayInSeconds);
        }

        public void SelectByValue(
            By by, 
            string optionValueToSelect,
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null)
        {
            WithRetry(() =>
            {
                MoveTo(by);

                var selectElement = FindElement(by, timeoutInSeconds, retryDelayInSeconds);

                var options = selectElement.FindElements(By.TagName("option"));
                var optionToSelect = options.FirstOrDefault(x => x.GetAttribute("value") == optionValueToSelect);

                if (optionToSelect == null)
                {
                    throw new SeleniumShieldDriverException(
                        $"Could not find option with value '{optionValueToSelect}', for select element");
                }

                MoveTo(optionToSelect, timeoutInSeconds, retryDelayInSeconds);
                optionToSelect.Click();
            }, timeoutInSeconds, retryDelayInSeconds);
        }

        public void SelectRandomValue(
            By by, 
            double? timeoutInSeconds = null, 
            double? retryDelayInMilliseconds = null)
        {
            WithRetry(() =>
            {
                var selectElement = FindElement(by);
                var options = selectElement.FindElements(By.TagName("option"));

                var random = new Random();

                var randomIndex = random.Next(1, options.Count);

                var optionToSelect = options[randomIndex];

                optionToSelect.Click();
            }, timeoutInSeconds, retryDelayInMilliseconds);
        }

        public void SelectByIndex(
            By by,
            int index,
            double? timeoutInSeconds = null,
            double? retryDelayInMilliseconds = null)
        {
            WithRetry(() =>
            {
                var selectElement = FindElement(by);
                var options = selectElement.FindElements(By.TagName("option"));

                var optionToSelect = options[index];

                optionToSelect.Click();
            }, timeoutInSeconds, retryDelayInMilliseconds);
        }

        public T WithUnsafeDriver<T>(
            Func<IWebDriver, T> action, 
            double? timeoutInSeconds = null, 
            double? retryDelayInSeconds = null, 
            bool throwOnTimeout = true)
        {
            return WithRetry(() => action(_driverToShield), timeoutInSeconds, retryDelayInSeconds, throwOnTimeout);
        }

        public void WithUnsafeDriver(
            Action<IWebDriver> action, 
            double? timeoutInSeconds = null, 
            double? retryDelayInSeconds = null,
            bool throwOnTimeout = true)
        {
            WithRetry(() => action(_driverToShield), timeoutInSeconds, retryDelayInSeconds, throwOnTimeout);
        }

        public T WithRetry<T>(
            Func<T> func, 
            double? timeoutInSeconds = null,
            double? retryDelayInSeconds = null,
            bool throwOnTimeout = true)
        {
            T result = default(T);

            WithRetry(() =>
            {
                result = func();
            },
                timeoutInSeconds,
                retryDelayInSeconds,
                throwOnTimeout);

            return result;
        }

        public void WithRetry(
            Action action, 
            double? timeoutInSeconds = null, 
            double? retryDelayInSeconds = null, 
            bool throwOnTimeout = true)
        {
            try
            {
                _actionRunner.Execute(() =>
                {
                    RetryingTaskRunner.ExecuteWithRetry(
                        action,
                        GetTimeoutInMilliseconds(timeoutInSeconds),
                        GetRetryDelainInMilliseconds(retryDelayInSeconds),
                        throwOnTimeout);

                }, _options.SleepTimeBetweenActionsInMilliseconds);
            }
            catch (Exception ex)
            {
                // Take screenshot

                LogWarning("WithRetry", $"Exception occurred: {ex}");

                throw;
            }
        }

        public void SetCheckpoint(Action resetAction = null)
        {
            _actionRunner.SetCheckpoint(resetAction);
        }

        private IWebElement TryFindElementUnsafe(By by)
        {
            try
            {
                return _driverToShield.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        private IWebElement FindElement(By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            return WithUnsafeDriver(driver => driver.FindElement(by), timeoutInSeconds, retryDelayInSeconds);
        }

        private ReadOnlyCollection<IWebElement> FindElements(By by, double? timeoutInSeconds = null, double? retryDelayInSeconds = null)
        {
            return WithUnsafeDriver(driver => driver.FindElements(by), timeoutInSeconds, retryDelayInSeconds);
        }

        private int GetRetryDelainInMilliseconds(double? retryDelayInSeconds)
        {
            return TimeConverter.ToMilliseconds(retryDelayInSeconds ?? _options.DefaultRetryDelayInSeconds);
        }

        private int GetTimeoutInMilliseconds(double? timeoutInSeconds)
        {
            return TimeConverter.ToMilliseconds(timeoutInSeconds ?? _options.DefaultTimeoutInSeconds);
        }

        private double? GetSubTimeoutInSeconds(double? totalTimeoutInSeconds, int actionCount)
        {
            var safeTotalTimeoutInSeconds = totalTimeoutInSeconds ?? _options.DefaultTimeoutInSeconds;

            return Math.Min(3, (safeTotalTimeoutInSeconds / actionCount));
        }
    }
}
