using System;
using System.Collections.Generic;
using NUnit.Framework;
using SeleniumShield.Driver;
using SeleniumShield.Driver.Checkpointing;
using SeleniumShield.Tests.Flows;
using SeleniumShield.Tests.Infrastructure;

namespace SeleniumShield.Tests.ActionExecution
{
    [TestFixture]
    public class ActionRunnerTests
    {
        private ActionRunner _runner;
        private List<int> _items;

        [SetUp]
        public void SetUp()
        {
            _runner = new ActionRunner();
            _items = new List<int>();
        }

        [Test]
        public void Runner_executes_specified_actions()
        {
            _runner.Execute(() => _items.Add(1));

            CollectionAssert.AreEqual(new [] { 1 }, _items);
        }

        [Test]
        public void Runner_executes_actions_in_the_given_sequence()
        {
            _runner.Execute(() => _items.Add(1));
            _runner.Execute(() => _items.Add(2));
            _runner.Execute(() => _items.Add(3));

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, _items);
        }

        [Test]
        public void Runner_wihout_checkpoint_aborts_execution_on_failure()
        {
            try
            {
                _runner.Execute(() => _items.Add(1));
                _runner.Execute(ConfigurableResultAction.CreateFailing().Invoke);
                _runner.Execute(() => _items.Add(3));
            }
            catch (PreconfiguredFailureException)
            {
            }
            
            CollectionAssert.AreEqual(new[] { 1 }, _items);
        }

        [Test]
        [ExpectedException(typeof(PreconfiguredFailureException))]
        public void Runner_without_snapshot_rethrows_exception()
        {
            _runner.Execute(ConfigurableResultAction.CreateFailing().Invoke);
        }

        [Test]
        public void Runner_with_checkpoint_resets_and_reruns_all_actions_after_checkpoint()
        {
            _runner.AllowedRestoreCountPerCheckpoint = 2;

            var singleFailAction = ConfigurableResultAction.Create(() => _items.Add(4))
                .QueueFailures(2)
                .QueueSuccessForAllSubsequentCalls();

            _runner.Execute(() => _items.Add(1));
            _runner.SetCheckpoint(() => _items.Add(100));
            _runner.Execute(() => _items.Add(2));
            _runner.Execute(() => _items.Add(3));
            _runner.Execute(singleFailAction.Invoke);
            _runner.Execute(() => _items.Add(5));
            _runner.Execute(() => _items.Add(6));

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 100, 2, 3, 100, 2, 3, 4, 5, 6 }, _items);
        }

        [Test]
        public void Runner_with_checkpoint_fails_if_action_fails_more_than_allowed_number_of_restores()
        {
            _runner.AllowedRestoreCountPerCheckpoint = 2;

            Exception actualException = null;

            try
            {
                _runner.Execute(() => _items.Add(1));
                _runner.SetCheckpoint(() => _items.Add(100));
                _runner.Execute(() => _items.Add(2));
                _runner.Execute(() => _items.Add(3));
                _runner.Execute(ConfigurableResultAction.CreateFailing().Invoke);
                _runner.Execute(() => _items.Add(5));
                _runner.Execute(() => _items.Add(6));
            }
            catch (PreconfiguredFailureException ex)
            {
                actualException = ex;
            }
            
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 100, 2, 3, 100, 2, 3 }, _items);
            Assert.IsNotNull(actualException);
        }

        [Test]
        public void Runner_can_restore_same_checkpoint_twice_when_failure_occurs_earlier_the_second_time_around()
        {
            _runner.AllowedRestoreCountPerCheckpoint = 3;


            var failsSecondTime = ConfigurableResultAction.Create(() => _items.Add(4))
                .QueueSuccess()
                .QueueFailure()
                .QueueSuccessForAllSubsequentCalls();

            var failsFirstTime = ConfigurableResultAction.Create(() => _items.Add(5))
                .QueueFailure()
                .QueueSuccessForAllSubsequentCalls();

            _runner.Execute(() => _items.Add(1));
            _runner.SetCheckpoint(() => _items.Add(100));
            _runner.Execute(() => _items.Add(2));
            _runner.Execute(() => _items.Add(3));
            _runner.Execute(failsSecondTime.Invoke);
            _runner.Execute(failsFirstTime.Invoke);
            _runner.Execute(() => _items.Add(6));
            _runner.Execute(() => _items.Add(7));

            AreEqual(new[] { 1, 2, 3, 4, 100, 2, 3, 100, 2, 3, 4, 5, 6, 7 }, _items);
        }


        [Test]
        public void Runner_only_restores_to_last_checkpoint()
        {
            _runner.AllowedRestoreCountPerCheckpoint = 3;

            var failsSecondTime = ConfigurableResultAction.Create(() => _items.Add(4))
                .QueueSuccess()
                .QueueFailure()
                .QueueSuccessForAllSubsequentCalls();

            var failsFirstTime1 = ConfigurableResultAction.Create(() => _items.Add(6))
                .QueueFailure()
                .QueueSuccessForAllSubsequentCalls();

            var failsFirstTime2 = ConfigurableResultAction.Create(() => _items.Add(7))
                .QueueFailure()
                .QueueSuccessForAllSubsequentCalls();

            var failsTwice = ConfigurableResultAction.Create(() => _items.Add(9))
                .QueueFailures(2)
                .QueueSuccessForAllSubsequentCalls();

            _runner.Execute(() => _items.Add(1));

            _runner.SetCheckpoint(() => _items.Add(100));

            _runner.Execute(() => _items.Add(2));
            _runner.Execute(() => _items.Add(3));
            _runner.Execute(failsSecondTime.Invoke);
            _runner.Execute(() => _items.Add(5));
            _runner.Execute(failsFirstTime1.Invoke);

            _runner.SetCheckpoint(() => _items.Add(200));

            _runner.Execute(failsFirstTime2.Invoke);

            _runner.SetCheckpoint(() => _items.Add(300));

            _runner.Execute(() => _items.Add(8));
            _runner.Execute(failsTwice.Invoke);
            _runner.Execute(() => _items.Add(10));
            _runner.Execute(() => _items.Add(11));


            AreEqual(new[] { 1, 2, 3, 4, 5, 100, 2, 3, 100, 2, 3, 4, 5, 6, 200, 7, 8, 300, 8, 300, 8, 9, 10, 11 }, _items);
        }

        private void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var expectedString = string.Join(", ", expected);
            var actualString = string.Join(", ", actual);

            var message = $"Expected: {expectedString}\r\nActual: {actualString}\r\n";
            CollectionAssert.AreEqual(expected, actual, message);

        }
    }
}