using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumShield.Exceptions;
using SeleniumShield.Tests.Fakes;

namespace SeleniumShield.Tests
{
    [TestFixture]
    public class AutomationFlowRunnerTests
    {
        private AutomationFlowRunner _runner;
        private AutomationFlowRunnerOptions _options;

        [SetUp]
        public void SetUp()
        {
            _options = new AutomationFlowRunnerOptions(() => new FakeWebDriver())
            {
                MaxAllowedStepFailCount = 4
            };

            _runner = new AutomationFlowRunner(_options);
        }

        [Test]
        public void Runner_executes_steps_in_the_order_in_which_they_were_appended()
        {
            var stepExecutions = new List<int>();

            _runner.AppendStep("Step 1", driver => stepExecutions.Add(1));
            _runner.AppendStep("Step 2", driver => stepExecutions.Add(2));
            _runner.AppendStep("Step 3", driver => stepExecutions.Add(3));

            _runner.Execute();

            Assert.AreEqual(3, stepExecutions.Count);
            Assert.AreEqual(1, stepExecutions[0]);
            Assert.AreEqual(2, stepExecutions[1]);
            Assert.AreEqual(3, stepExecutions[2]);
        }

        [Test]
        public void Result_for_the_run_reflects_the_step_execution_order()
        {
            _runner.AppendStep("Step 1", driver => { });
            _runner.AppendStep("Step 2", driver => { });
            _runner.AppendStep("Step 3", driver => { });

            var result = _runner.Execute();

            var automationStepRunResults = result.StepResults.ToList();

            Assert.AreEqual(3, automationStepRunResults.Count);
            Assert.AreEqual("Step 1", automationStepRunResults[0].Description);
            Assert.AreEqual("Step 2", automationStepRunResults[1].Description);
            Assert.AreEqual("Step 3", automationStepRunResults[2].Description);
        }

        [Test]
        public void Step_results_include_statistics_of_the_number_of_failed_attempts()
        {
            _runner.AppendStep(CreateSuccessStep());
            _runner.AppendStep(CreateFailingStep(failCountBeforeSucceeding: 3));

            var result = _runner.Execute();

            var stepResults = result.StepResults.ToList();

            Assert.AreEqual(2, stepResults.Count);
            Assert.AreEqual(0, stepResults[0].FailedAttemptCount);
            Assert.AreEqual(3, stepResults[1].FailedAttemptCount);
        }

        [Test]
        public void Step_results_include_information_about_all_exceptions_that_occured_for_a_given_step()
        {
            _runner.AppendStep(CreateFailingStep(failCountBeforeSucceeding: 3));

            var result = _runner.Execute();

            var stepResults = result.StepResults.ToList();

            Assert.AreEqual(1, stepResults.Count);

            var failedExecutions = stepResults[0].FailedExecutions.ToList();

            Assert.AreEqual(3, failedExecutions.Count);
            Assert.IsTrue(failedExecutions[0].Exception.Message.Contains("1 of 3"), "Unexpected step execution exception");
            Assert.IsTrue(failedExecutions[1].Exception.Message.Contains("2 of 3"), "Unexpected step execution exception");
            Assert.IsTrue(failedExecutions[2].Exception.Message.Contains("3 of 3"), "Unexpected step execution exception");
        }

        [Test]
        public void Failing_step_is_not_executed_more_than_the_max_allowed_number_times()
        {
            _runner.AppendStep(CreateFailingStep(failCountBeforeSucceeding: 100));

            var result = _runner.Execute();

            var stepResults = result.StepResults.ToList();

            Assert.AreEqual(1, stepResults.Count);
            Assert.AreEqual(_options.MaxAllowedStepFailCount, stepResults[0].FailedAttemptCount);
        }

        [Test]
        public void Steps_which_did_not_succeed_within_allowed_number_of_attempts_are_marked_as_failed()
        {
            _runner.AppendStep(CreateFailingStep(failCountBeforeSucceeding: 100));

            var result = _runner.Execute();

            var stepResults = result.StepResults.ToList();

            Assert.AreEqual(1, stepResults.Count);
            Assert.IsFalse(stepResults[0].WasSuccessful);
        }

        [Test]
        public void Steps_which_succeed_within_allowed_number_of_attempts_are_marked_as_successful()
        {
            _runner.AppendStep(CreateFailingStep(failCountBeforeSucceeding: 1));

            var result = _runner.Execute();

            var stepResults = result.StepResults.ToList();

            Assert.AreEqual(1, stepResults.Count);
            Assert.IsTrue(stepResults[0].WasSuccessful);
        }

        [Test]
        public void Total_attempt_count_for_step_which_succeeds_on_the_first_attempt_is_1()
        {
            _runner.AppendStep(CreateSuccessStep());

            var result = _runner.Execute();

            var stepResults = result.StepResults.ToList();

            Assert.AreEqual(1, stepResults.Count);
            Assert.AreEqual(1, stepResults[0].TotalAttemptCount);
        }

        [Test]
        public void Total_attempt_count_for_step_which_fails_is_the_number_of_failed_attempts()
        {
            _runner.AppendStep(CreateFailingStep(failCountBeforeSucceeding:100));

            var result = _runner.Execute();

            var stepResults = result.StepResults.ToList();

            Assert.AreEqual(1, stepResults.Count);
            Assert.AreEqual(_options.MaxAllowedStepFailCount, stepResults[0].TotalAttemptCount);
        }

        [Test]
        public void Total_attempt_count_for_step_which_eventually_succeeds_is_the_number_of_failed_attempts_plus_the_successful_attempt()
        {
            _runner.AppendStep(CreateFailingStep(failCountBeforeSucceeding:2));

            var result = _runner.Execute();

            var stepResults = result.StepResults.ToList();

            Assert.AreEqual(1, stepResults.Count);
            Assert.AreEqual(3, stepResults[0].TotalAttemptCount);
        }

        [Test]
        public void Result_report_can_be_generated_for_run_with_both_failed_and_succeded_steps()
        {
            _runner.AppendStep(CreateSuccessStep());
            _runner.AppendStep(CreateFailingStep(failCountBeforeSucceeding: 2));
            _runner.AppendStep(CreateFailingStep(failCountBeforeSucceeding: 1000));

            var result = _runner.Execute();

            // Just generate the report to make sure that nothing blows up. Not worth it to check the actual
            // string contents ATM
            var resultReport = result.GetResultReport();

            Assert.IsNotNull(resultReport);
        }

        private AutomationStep CreateSuccessStep()
        {
            return new AutomationStep("Success step", driver => { });
        }

        [TearDown]
        public void TearDown()
        {
            _runner.Dispose();
        }

        private AutomationStep CreateFailingStep(int failCountBeforeSucceeding)
        {
            var remainingFailuresBeforeSuccess = failCountBeforeSucceeding;

            Action<IWebDriver> executeAction = driver =>
            {
                remainingFailuresBeforeSuccess -= 1;

                if (remainingFailuresBeforeSuccess >= 0)
                {
                    throw new SeleniumShieldException(
                        "Failing step: Set up to fail ({0} of {1})",
                        failCountBeforeSucceeding - remainingFailuresBeforeSuccess, 
                        failCountBeforeSucceeding);
                }
            };

            return new AutomationStep("Failing step", executeAction);
        }

        private class ParentFlow : AutomationFlow
        {
            private readonly AutomationFlow[] _childFlows;

            public ParentFlow(params AutomationFlow[] childFlows)
            {
                _childFlows = childFlows;
            }

            public override IEnumerable<AutomationStep> GetSteps()
            {
                var steps = new List<AutomationStep>();

                foreach (var automationFlow in _childFlows)
                {
                    steps.AddRange(automationFlow.GetSteps());
                }

                return steps;
            }
        }

        private class FailingChildFlow : AutomationFlow
        {
            private int _remainingFailuresBeforeSuccess;

            public FailingChildFlow(int failCountBeforeSucceeding)
            {
                _remainingFailuresBeforeSuccess = failCountBeforeSucceeding;
            }

            public override IEnumerable<AutomationStep> GetSteps()
            {
                yield return new AutomationStep("Failing child flow step", driver =>
                {
                    if (_remainingFailuresBeforeSuccess > 0)
                    {
                        throw new SeleniumShieldException("Set up to fail");
                    }

                    _remainingFailuresBeforeSuccess -= 1;
                });
            }
        }

        private class SuccessfulChildFlow : AutomationFlow
        {
            public override IEnumerable<AutomationStep> GetSteps()
            {
                yield return new AutomationStep("Successful child step", driver => { });
            }
        }
    }
}