using System;
using OpenQA.Selenium;

namespace SeleniumShield
{
    public class AutomationStep
    {
        private readonly string _description;
        private readonly Action<IWebDriver> _executeAction;
        private readonly Action<IWebDriver> _resetAction;

        public AutomationStep(string description, Action<IWebDriver> executeAction, Action<IWebDriver> resetAction = null)
        {
            _description = description;
            _executeAction = executeAction;
            _resetAction = resetAction;
        }

        public void Execute(IWebDriver driver)
        {
            _executeAction(driver);
        }

        public virtual string Description
        {
            get { return _description; }
        }

        public void Reset(IWebDriver driver)
        {
            if (_resetAction != null)
            {
                _resetAction(driver);
            }
        }
    }
}