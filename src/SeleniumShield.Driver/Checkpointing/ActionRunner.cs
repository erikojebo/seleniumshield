using System;
using System.Collections.Generic;
using System.Linq;

namespace SeleniumShield.Driver.Checkpointing
{
    public class ActionRunner
    {
        private readonly List<Checkpoint> _checkpoints = new List<Checkpoint>();

        public void Execute(Action action)
        {
            if (_checkpoints.Any())
                _checkpoints.Last().AddExecutedAction(action);

            ExecuteWithCheckpointRestore(action);
        }

        public void ExecuteWithCheckpointRestore(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                if (CanRestoreToCheckpoint())
                {
                    RerunFromLastCheckpoint();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CanRestoreToCheckpoint()
        {
            return _checkpoints.Any() && _checkpoints.Last().CanRestore();
        }

        private void RerunFromLastCheckpoint()
        {
            var checkpoint = _checkpoints.Last();

            ExecuteWithCheckpointRestore(checkpoint.ReplayExecuted);
        }

        public int AllowedRestoreCountPerCheckpoint { get; set; }

        public void SetCheckpoint(Action resetAction)
        {
            _checkpoints.Add(new Checkpoint(AllowedRestoreCountPerCheckpoint, resetAction));
        }

        public void RestoreLastCheckpoint()
        {
            if (_checkpoints.Any())
                _checkpoints.Last().ReplayExecuted();
        }
    }
}