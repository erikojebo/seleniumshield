using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace SeleniumShield
{
    public class AutomationFlowRunner : IDisposable
    {
        private readonly AutomationFlowRunnerOptions _options;
        private readonly IList<Func<IWebDriver>> _driverFactoryFuncs;
        private readonly IList<IWebDriver> _drivers;
        private readonly List<AutomationStep> _steps;

        public AutomationFlowRunner() : this(new AutomationFlowRunnerOptions())
        {
        }

        public AutomationFlowRunner(AutomationFlowRunnerOptions options)
        {
            _options = options;
            _driverFactoryFuncs = options.WebDriverFactories.ToList();

            if (!_driverFactoryFuncs.Any())
            {
                _driverFactoryFuncs.Add(() => new FirefoxDriver());
            }

            _steps = new List<AutomationStep>();
            _drivers = new List<IWebDriver>();
        }

        public void AppendFlow(AutomationFlow flow)
        {
            var steps = flow.GetSteps();

            foreach (var automationStep in steps)
            {
                AppendStep(automationStep);
            }
        }

        public void AppendStep(string description, Action<IWebDriver> execute, Action<IWebDriver> reset = null)
        {
            var step = new AutomationStep(description, execute, reset);
            AppendStep(step);
        }

        public void AppendStep(AutomationStep stepDefinition)
        {
            _steps.Add(stepDefinition);
        }

        public AutomationRunResult Execute()
        {
            var automationRunResult = new AutomationRunResult();

            var driver = _driverFactoryFuncs[0]();
            _drivers.Add(driver);

            foreach (var step in _steps)
            {
                var stepRunResult = new AutomationStepRunResult(step)
                {
                    StartTime = DateTime.Now
                };

                for (int i = 0; i < _options.MaxAllowedStepFailCount; i++)
                {
                    stepRunResult.TotalAttemptCount += 1;

                    try
                    {
                        step.Execute(driver);
                        stepRunResult.WasSuccessful = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        stepRunResult.AppendFailedExecution(DateTime.Now, ex);
                    }
                }

                stepRunResult.EndTime = DateTime.Now;
                automationRunResult.AppendStepResult(stepRunResult);
            }

            return automationRunResult;
        }

        public void Dispose()
        {
            foreach (var webDriver in _drivers)
            {
                webDriver.Quit();
                webDriver.Dispose();
            }
        }
    }
}