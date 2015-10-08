using System.Collections.Generic;

namespace SeleniumShield.Orchestration
{
    public class AutomationOrchestrator
    {
        private readonly AutomationFlowRunnerOptions _options;
        private readonly List<AutomationFlow> _automationFlows = new List<AutomationFlow>();

        public AutomationOrchestrator(AutomationFlowRunnerOptions options, params AutomationFlow[] automationFlows)
        {
            _options = options;
            _automationFlows.AddRange(automationFlows);
        }

        public void AddFlow(AutomationFlow flow)
        {
            _automationFlows.Add(flow);
        }

        public void Run()
        {
            foreach (var automationFlow in _automationFlows)
            {
                var runner = new AutomationFlowRunner(_options);

                runner.AppendFlow(automationFlow);

                var result = runner.Execute();

                WriteResultToOutput(result);
            }
        }

        private void WriteResultToOutput(AutomationRunResult result)
        {
            var resultReport = result.GetResultReport();
            _options.ResultListener.Output(resultReport);
        }
    }
}