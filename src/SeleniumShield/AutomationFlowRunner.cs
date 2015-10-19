using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using SeleniumShield.Output;

namespace SeleniumShield
{
    public delegate void StepCompletedHandler(AutomationStepRunResult result, int stepIndex, int totalStepCount);

    public class AutomationFlowRunner : IDisposable
    {
        private readonly AutomationFlowRunnerOptions _options;
        private readonly IList<Func<IWebDriver>> _driverFactoryFuncs;
        private readonly IList<IWebDriver> _drivers;
        private readonly List<AutomationStep> _steps;
        private readonly List<IResultListener> _resultListeners;

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
            _resultListeners = new List<IResultListener>() { options.ResultListener };
        }

        public event StepCompletedHandler StepCompleted;

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

        public void AppendStep(string description, Action<IWebDriver, dynamic> execute, Action<IWebDriver> reset = null)
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
            _resultListeners.Add(automationRunResult);

            var driver = _driverFactoryFuncs[0]();
            _drivers.Add(driver);

            var state = new ExpandoObject();

            foreach (var step in _steps)
            {
                var stepRunResult = ExecuteStep(step, driver, state);
                automationRunResult.AppendStepResult(stepRunResult);

                if (!stepRunResult.WasSuccessful)
                    return automationRunResult;

                FireStepCompleted(stepRunResult, _steps.IndexOf(step), _steps.Count);
            }

            _resultListeners.Remove(automationRunResult);

            return automationRunResult;
        }

        private AutomationStepRunResult ExecuteStep(AutomationStep step, IWebDriver driver, dynamic state)
        {
            var stepRunResult = new AutomationStepRunResult(step)
            {
                StartTime = DateTime.Now
            };

            Log("[{0}] Starting step '{1}'...", DateTime.Now, step.Description);

            for (int i = 0; i < _options.MaxAllowedStepFailCount; i++)
            {
                stepRunResult.TotalAttemptCount += 1;

                try
                {
                    step.Execute(driver, state);
                    stepRunResult.WasSuccessful = true;

                    Log("[{0}]    Success", DateTime.Now);

                    break;
                }
                catch (Exception ex)
                {
                    stepRunResult.AppendFailedExecution(DateTime.Now, ex);

                    Log("[{0}]    Failure #{1}: {2}", DateTime.Now, stepRunResult.FailedAttemptCount, ex.Message);
                }

            }

            var resultText = stepRunResult.WasSuccessful ? "succeeded" : "failed";

            Log("[{0}] Step '{3}' {1} after {2} attempts.", DateTime.Now, resultText, stepRunResult.TotalAttemptCount, step.Description);

            stepRunResult.EndTime = DateTime.Now;

            return stepRunResult;
        }

        public void Dispose()
        {
            foreach (var webDriver in _drivers)
            {
                webDriver.Quit();
                webDriver.Dispose();
            }
        }

        private void Log(string message, params object[] formatParams)
        {
            foreach (var listener in _resultListeners)
            {
                listener.OutputLine(message, formatParams);
            }
        }

        protected virtual void FireStepCompleted(AutomationStepRunResult result, int stepindex, int totalstepcount)
        {
            StepCompleted?.Invoke(result, stepindex, totalstepcount);
        }
    }
}