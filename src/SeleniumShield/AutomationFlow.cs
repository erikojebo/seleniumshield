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
            AddStep(new AutomationStep(description, execute, reset));
        }

        protected void AddFlow(AutomationFlow flow)
        {
            foreach(var step in flow.GetSteps())
            {
                AddStep(step);
            }
        }

        protected void AddStep(AutomationStep step)
        {
            _steps.Add(step);
        }
    }
}
