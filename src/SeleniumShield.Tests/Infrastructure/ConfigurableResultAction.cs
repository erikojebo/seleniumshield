using System;
using System.Collections.Generic;
using System.Linq;

namespace SeleniumShield.Tests.Infrastructure
{
    public class ConfigurableResultAction
    {
        private readonly Action _action;
        private readonly Queue<bool> _outcomeQueue = new Queue<bool>();
        private bool? _defaultOutcome;

        public ConfigurableResultAction(Action action)
        {
            _action = action;
        }

        public ConfigurableResultAction QueueFailure()
        {
            _outcomeQueue.Enqueue(false);

            return this;
        }

        public ConfigurableResultAction QueueSuccess()
        {
            _outcomeQueue.Enqueue(true);

            return this;
        }

        public ConfigurableResultAction QueueSuccessForAllSubsequentCalls()
        {
            _defaultOutcome = true;
            return this;
        }

        public ConfigurableResultAction QueueFailureForAllSubsequentCalls()
        {
            _defaultOutcome = false;
            return this;
        }

        public ConfigurableResultAction QueueFailures(int count)
        {
            for (int i = 0; i < count; i++)
            {
                QueueFailure();
            }

            return this;
        }

        public ConfigurableResultAction QueueSuccesses(int count)
        {
            for (int i = 0; i < count; i++)
            {
                QueueSuccess();
            }

            return this;
        }

        public void Invoke()
        {
            if (_outcomeQueue.Any())
                TriggerOutcome(_outcomeQueue.Dequeue());
            else if (_defaultOutcome != null)
                TriggerOutcome(_defaultOutcome.Value);
            else
                throw new Exception("No outcome configured");
        }

        private void TriggerOutcome(bool shouldSucceed)
        {
            if (shouldSucceed)
                _action();
            else
                throw new PreconfiguredFailureException("Configured to fail");
        }

        public static ConfigurableResultAction Create(Action action)
        {
            return new ConfigurableResultAction(action);
        }

        public static ConfigurableResultAction CreateFailing()
        {
            return new ConfigurableResultAction(() => {}).QueueFailureForAllSubsequentCalls();
        }

        public static ConfigurableResultAction CreateSucceeding(Action action)
        {
            return new ConfigurableResultAction(action).QueueSuccessForAllSubsequentCalls();
        }
    }
}