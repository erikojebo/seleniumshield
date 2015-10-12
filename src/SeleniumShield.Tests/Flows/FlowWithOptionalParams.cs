using SeleniumShield.Metadata;

namespace SeleniumShield.Tests.Flows
{
    [UIExecutable(Description = "Has one constructor with both required and optional parameters")]
    public class FlowWithOptionalParams : AutomationFlow
    {
        public FlowWithOptionalParams(string requiredValue, string email = null, string password = "admin", int maxCount = 3)
        {
            AddStep($"Do nothing. 'requiredValue': {requiredValue}, 'email': {email}, 'password': {password}, 'maxCount': {maxCount}", driver => {});
        }
    }
}