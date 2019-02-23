using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.Shared.Scheduling
{
    public class TaskCompleteEventArgs<T> : EventArgs
    {
        public T Value { get; }

        public TaskCompleteEventArgs(T value) => Value = value;
    }
}
