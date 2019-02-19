/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Shared.Config;
using Wobble;

namespace Quaver.Shared.Scheduling
{
    internal static class CommonTaskScheduler
    {
        /// <summary>
        ///     The the that the task scheduler ran.
        /// </summary>
        internal static long LastRunTime { get; private set; }

        /// <summary>
        ///     The list of currently queued common tasks.
        /// </summary>
        private static List<CommonTask> QueuedTasks { get; } = new List<CommonTask>();

        /// <summary>
        ///     If the tasks are currently running.
        /// </summary>
        private static bool IsRunning { get; set; }

        /// <summary>
        ///     Adds a common task to the queue
        /// </summary>
        /// <param name="task"></param>
        internal static void Add(CommonTask task)
        {
            if (QueuedTasks.All(x => x != task))
                QueuedTasks.Add(task);
        }

        /// <summary>
        ///     Runs all scheduled common tasks on a separate thread.
        /// </summary>
        internal static void Run()
        {
            if (GameBase.Game.TimeRunning - LastRunTime <= 5000 || IsRunning || QueuedTasks.Count == 0)
                return;

            var taskList = new List<Task>();

            // Thread that completes all the tasks.
            var taskThread = new Thread(() =>
            {
                foreach (var task in taskList)
                    task.Start();

                // Wait for all the tasks to complete.
                Task.WaitAll(taskList.ToArray());

                // Clear the queue of commonly ran tasks.
                QueuedTasks.Clear();
                IsRunning = false;

                // Set the last run time.
                LastRunTime = GameBase.Game.TimeRunning;
            });

            // Add all common tasks to the queue.
            foreach (var task in QueuedTasks)
            {
                switch (task)
                {
                    // Writes the user's cnfig file.
                    case CommonTask.WriteConfig:
                        taskList.Add(new Task(async () => await ConfigManager.WriteConfigFileAsync()));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Start the thread once we have all the queued tasks setup.
            taskThread.Start();
            IsRunning = true;
        }
    }

    /// <summary>
    ///
    /// </summary>
    internal enum CommonTask
    {
        WriteConfig
    }
}
