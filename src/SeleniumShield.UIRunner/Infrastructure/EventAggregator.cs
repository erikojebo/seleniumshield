using System;

namespace SeleniumShield.UIRunner.Infrastructure
{
    public class EventAggregator
    {
        public static event Action<string> FlowExecutionFailed;

        public static void FireFlowExecutionFailed(string message)
        {
            FlowExecutionFailed?.Invoke(message);
        }
    }
}