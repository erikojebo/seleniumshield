using System.Linq;
using NUnit.Framework;

namespace SeleniumShield.Tests
{
    [TestFixture]
    public class AutomationFlowTests
    {
        [Test]
        public void Adding_a_flow_adds_each_individual_step_to_the_base_flow()
        {
            var steps = new ParentFlow().GetSteps().ToList();

            Assert.AreEqual(6, steps.Count);
            Assert.AreEqual("Parent step 1", steps[0].Description);
            Assert.AreEqual("Parent step 2", steps[1].Description);
            Assert.AreEqual("Child step 1", steps[2].Description);
            Assert.AreEqual("Child step 2", steps[3].Description);
            Assert.AreEqual("Parent step 3", steps[4].Description);
            Assert.AreEqual("Parent step 4", steps[5].Description);
        }

        private class ParentFlow : AutomationFlow
        {
            public ParentFlow()
            {
                AddStep("Parent step 1", d => {});
                AddStep("Parent step 2", d => {});

                AddFlow(new ChildFlow());

                AddStep("Parent step 3", d => {});
                AddStep("Parent step 4", d => {});
            }
        }

        private class ChildFlow : AutomationFlow
        {
            public ChildFlow()
            {
                AddStep("Child step 1", d => {});
                AddStep("Child step 2", d => {});
            }
        }
    }
}