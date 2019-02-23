using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Quaver.Shared.Scheduling
{
    public class TaskHandler<T>
    {
        private Action<T> Action { get; }

        /// <summary>
        ///     Event Invoked when the task gets completed
        /// </summary>
        public event EventHandler<TaskCompleteEventArgs<T>> OnCompleted;

        /// <summary>
        ///     Event Invoked when the task gets cancelled
        /// </summary>
        public event EventHandler<TaskCancelledEventArgs> OnCancelled;

        private CancellationTokenSource Source { get; set; }

        public TaskHandler(Action<T> action) => Action = action;

        private void Run()
        {
            // do stuff
            try
            {
                //var value = Action.Invoke();
                Source.Token.ThrowIfCancellationRequested();
                OnCompleted?.Invoke(typeof(TaskHandler<T>), new TaskCompleteEventArgs());
            }
            catch (OperationCanceledException e)
            {
                Cancel();
            }
        }

        private void Cancel() => OnCancelled?.Invoke(typeof(TaskHandler<T>), new TaskCancelledEventArgs());

        public void Dispose()
        {
            Cancel();
            OnCompleted = null;
            OnCancelled = null;
        }
    }
}
