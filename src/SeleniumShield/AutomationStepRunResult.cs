using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SeleniumShield
{
    public class AutomationStepRunResult
    {
        private readonly AutomationStep _step;
        private readonly List<FailedAutomationStepExecution> _failedExecutions;

        public AutomationStepRunResult(AutomationStep step)
        {
            _step = step;
            _failedExecutions = new List<FailedAutomationStepExecution>();
        }

        public string Description
        {
            get { return _step.Description; }
        }

        public int FailedAttemptCount
        {
            get { return _failedExecutions.Count; }
        }

        public void AppendFailedExecution(DateTime failureDateTime, Exception exception)
        {
            _failedExecutions.Add(new FailedAutomationStepExecution(failureDateTime, exception));
        }

        public IEnumerable<FailedAutomationStepExecution> FailedExecutions
        {
            get
            {
                // Make sure that the IEnumerable can't be cast back to a list
                return _failedExecutions.Skip(0);
            }
        }

        public DateTime StartTime { get; internal set; }
        public DateTime EndTime { get; internal set; }
        public bool WasSuccessful { get; internal set; }
        public int TotalAttemptCount { get; internal set; }
    }
}