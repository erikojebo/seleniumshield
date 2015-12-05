using System;
using System.Collections.Generic;
using System.Linq;

namespace SeleniumShield.Driver.Checkpointing
{
    internal class Checkpoint
    {
        private readonly int _allowedRestoreCount;
        private readonly Action _resetAction;
        private readonly List<Action> _executedActions = new List<Action>();
        private int _restoreCount;

        public Checkpoint(int allowedRestoreCount, Action resetAction)
        {
            _allowedRestoreCount = allowedRestoreCount;
            _resetAction = resetAction;
        }

        public bool CanRestore()
        {
            return _allowedRestoreCount > _restoreCount;
        }

        public void ReplayExecuted()
        {
            _restoreCount++;
            _resetAction?.Invoke();

            foreach (var executedAction in _executedActions.ToList())
            {
                executedAction();
            }
        }

        public void AddExecutedAction(Action action)
        {
            _executedActions.Add(action);
        }
    }
}