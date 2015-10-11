using System;
using SeleniumShield.Metadata;

namespace SeleniumShield.Tests.Flows
{
    [UIExecutable]
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