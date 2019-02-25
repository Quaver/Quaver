using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.Shared.Scheduling
{
    public class TaskCancelledEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     Input given to task before cancelled
        /// </summary>
        public T Input { get; }

        public TaskCancelledEventArgs (T input) => Input = input
    }
}
