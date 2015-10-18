using System;
using OpenQA.Selenium;

namespace SeleniumShield
{
    public class AutomationStep
    {
        private readonly Action<IWebDriver, dynamic> _executeAction;
        private readonly Action<IWebDriver> _resetAction;

        public AutomationStep(string description, Action<IWebDriver> executeAction, Action<IWebDriver> resetAction = null)
            : this(description, (driver, state) => executeAction(driver), resetAction)
        {
        }

        public AutomationStep(string description, Action<IWebDriver, dynamic> executeAction, Action<IWebDriver> resetAction = null)
        {
            Description = description;
            _executeAction = executeAction;
            _resetAction = resetAction;
        }

        public void Execute(IWebDriver driver, dynamic state)
        {
            _executeAction(driver, state);
        }

        public virtual string Description { get; }

        public void Reset(IWebDriver driver)
        {
            _resetAction?.Invoke(driver);
        }
    }
}