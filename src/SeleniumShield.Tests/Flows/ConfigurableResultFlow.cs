using System;
using SeleniumShield.Metadata;

namespace SeleniumShield.Tests.Flows
{
    [UIExecutable(DependencyGroup = "Testing", DependencyGroupOrder = 1)]
    public class ConfigurableResultFlow : AutomationFlow
    {
        public ConfigurableResultFlow(bool shouldSucceed)
        {
            AddStep("Step", driver =>
            {
                if (!shouldSucceed)
                {
                    throw new Exception("Set up to fail");
                }
            });
        }
    }
}