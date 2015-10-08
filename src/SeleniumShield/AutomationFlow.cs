using System.Collections.Generic;

namespace SeleniumShield
{
    public abstract class AutomationFlow
    {
        public virtual void Reset()
        {
        }

        public abstract IEnumerable<AutomationStep> GetSteps();
    }
}
