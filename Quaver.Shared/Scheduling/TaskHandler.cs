using System;
using System.Threading;
using Wobble.Logging;

namespace Quaver.Shared.Scheduling
{
    public class TaskHandler<T, TResult>
    {
        private Func<T, TResult> Function { get; }

        /// <summary>
        ///     Event Invoked when the task gets completed.
        /// </summary>
        public event EventHandler<TaskCompleteEventArgs<T, TResult>> OnCompleted;

        /// <summary>
        ///     Event Invoked when the task gets cancelled.
        /// </summary>
        public event EventHandler<TaskCancelledEventArgs<T>> OnCancelled;

        /// <summary>
        ///     Used to handle Task Cancellation.
        /// </summary>
        public CancellationTokenSource Source { get; private set; } = new CancellationTokenSource();

        public TaskHandler(Func<T, TResult> function) => Function = function;

        /// <summary>
        ///     This method will cancel the previous task and run a new task.
        /// </summary>
        /// <param name="input"></param>
        public void Run(T input)
        {
            Source.Cancel();
            Source.Dispose();
            Source = new CancellationTokenSource();
            try
            {
                var token = Source.Token;
                var result = Function(input);
                token.ThrowIfCancellationRequested();
                OnCompleted?.Invoke(typeof(TaskHandler<T, TResult>), new TaskCompleteEventArgs<T,TResult>(input, result));
            }
            catch (OperationCanceledException e)
            {
                Cancel(input);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     This method is called when a task is cancelled.
        /// </summary>
        /// <param name="input"></param>
        private void Cancel(T input) => OnCancelled?.Invoke(typeof(TaskHandler<T, TResult>), new TaskCancelledEventArgs<T>(input));

        /// <summary>
        ///     Should be called when Task Handler will no longer be used.
        /// </summary>
        public void Dispose()
        {
            OnCompleted = null;
            OnCancelled = null;
        }
    }
}
