using System.Collections.Generic;

namespace SeleniumShield
{
    public interface IAutomationFlow
    {
        IEnumerable<AutomationStep> GetSteps();
        void Reset();
    }
}