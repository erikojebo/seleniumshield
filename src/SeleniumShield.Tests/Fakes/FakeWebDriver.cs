using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenQA.Selenium;

namespace SeleniumShield.Tests.Fakes
{
    public class FakeWebDriver : IWebDriver
    {
        public IWebElement FindElement(By @by)
        {
            return null;
        }

        public ReadOnlyCollection<IWebElement> FindElements(By @by)
        {
            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
        }

        public void Dispose()
        {
        }

        public void Close()
        {
        }

        public void Quit()
        {
        }

        public IOptions Manage()
        {
            return null;
        }

        public INavigation Navigate()
        {
            return null;
        }

        public ITargetLocator SwitchTo()
        {
            return null;
        }

        public string Url { get; set; }
        public string Title { get; private set; }
        public string PageSource { get; private set; }
        public string CurrentWindowHandle { get; private set; }
        public ReadOnlyCollection<string> WindowHandles { get; private set; }
    }
}