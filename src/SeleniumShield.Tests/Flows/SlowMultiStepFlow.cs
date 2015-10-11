using System.Threading;
using SeleniumShield.Metadata;

namespace SeleniumShield.Tests.Flows
{
    [UIExecutable(DependencyGroup = "Testing", DependencyGroupOrder = 2)]
    public class SlowMultiStepFlow : AutomationFlow
    {
        public SlowMultiStepFlow(int stepCount) : this(1000, stepCount)
        {
            
        }

        public SlowMultiStepFlow(int stepDelayInMilliseconds, int stepCount)
        {
            for (int i = 0; i < stepCount; i++)
            {
                AddStep($"Step {i}", driver => Thread.Sleep(stepDelayInMilliseconds));
            }
        }
    }
}