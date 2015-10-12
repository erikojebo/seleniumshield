using SeleniumShield.Metadata;

namespace SeleniumShield.Tests.Flows
{
    [UIExecutable(Description = "Has one constructor with both required and optional parameters")]
    public class FlowWithOptionalParams : AutomationFlow
    {
        public FlowWithOptionalParams(string requiredValue, string email = null, int maxCount = 3)
        {
            AddStep("Do nothing", driver => {});
        }
    }
}