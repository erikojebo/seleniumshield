using System;
using System.Collections.Generic;
using OpenQA.Selenium;

namespace SeleniumShield
{
    public abstract class AutomationFlow : IAutomationFlow
    {
        private readonly IList<AutomationStep> _steps =  new List<AutomationStep>();

        public virtual void Reset()
        {
        }

        public virtual IEnumerable<AutomationStep> GetSteps()
        {
            return _steps;
        }

        protected void AddStep(string description, Action<IWebDriver> execute, Action<IWebDriver> reset = null)
        {
            _steps.Add(new AutomationStep(description, execute, reset));
        }
    }
}
