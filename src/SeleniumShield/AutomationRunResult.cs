using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeleniumShield.Output;

namespace SeleniumShield
{
    public class AutomationRunResult : IResultListener
    {
        private readonly IList<AutomationStepRunResult> _stepResults;
        private StringBuilder _resultOutput;

        public AutomationRunResult()
        {
            _stepResults = new List<AutomationStepRunResult>();
            _resultOutput = new StringBuilder();
        }

        // Make sure the IEnumerable can't be cast back to a List
        public IEnumerable<AutomationStepRunResult> StepResults => _stepResults.Skip(0);

        public void AppendStepResult(AutomationStepRunResult automationStepRunResult)
        {
            _stepResults.Add(automationStepRunResult);
        }

        public string GetResultReport()
        {
            return _resultOutput.ToString();
        }

        public void OutputLine(string message, params object[] formatParams)
        {
            _resultOutput.AppendFormat(message, formatParams);
            _resultOutput.AppendLine();
        }
    }
}