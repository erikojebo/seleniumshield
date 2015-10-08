using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeleniumShield
{
    public class AutomationRunResult
    {
        private readonly IList<AutomationStepRunResult> _stepResults;

        public AutomationRunResult()
        {
            _stepResults = new List<AutomationStepRunResult>();
        }

        public IEnumerable<AutomationStepRunResult> StepResults
        {
            get
            {
                // Make sure the IEnumerable can't be cast back to a List
                return _stepResults.Skip(0);
            }
        }

        public void AppendStepResult(AutomationStepRunResult automationStepRunResult)
        {
            _stepResults.Add(automationStepRunResult);
        }

        public string GetResultReport()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < _stepResults.Count; i++)
            {
                sb.AppendFormat("[{0}] Starting step '{1}'...", _stepResults[i].StartTime, _stepResults[i].Description);
                sb.AppendLine();

                var failedExecutions = _stepResults[i].FailedExecutions.ToList();

                for (int j = 0; j < failedExecutions.Count(); j++)
                {
                    sb.AppendFormat("[{0}]    Failure #{1}: {2}", failedExecutions[j].FailureDateTime, j, failedExecutions[j].Exception.Message);
                    sb.AppendLine();
                }

                if (_stepResults[i].WasSuccessful && _stepResults[i].FailedAttemptCount > 0)
                {
                    sb.AppendFormat("[{0}]    Success", _stepResults[i].EndTime);
                    sb.AppendLine();
                }

                var resultText = _stepResults[i].WasSuccessful ? "succeeded" : "failed";

                sb.AppendFormat(
                    "[{0}] Step '{3}' {1} after {2} attempts.",
                    _stepResults[i].EndTime, 
                    resultText,
                    _stepResults[i].TotalAttemptCount, 
                    _stepResults[i].Description);

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}