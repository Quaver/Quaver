using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quaver.Config;
using Quaver.Logging;

namespace Quaver.Main
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
        private static List<CommonTask> Queued { get; } = new List<CommonTask>();

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
            if (Queued.All(x => x != task)) 
                Queued.Add(task);     
        }

        /// <summary>
        ///     Runs all scheduled common tasks on a separate thread.
        /// </summary>
        internal static void Run()
        {
            // If we're already running our tasks or there aren't any queued, then don't run.
            if (IsRunning || Queued.Count == 0)
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
                Queued.Clear();
                IsRunning = false;
                
                // Set the last run time.
                LastRunTime = GameBase.GameTime.ElapsedMilliseconds;
            });
            
            // Add all common tasks to the queue.
            for (var i = 0; i < Queued.Count; i++)
            {
                switch (Queued[i])
                {    
                    // Writes the user's cnfig file.
                    case CommonTask.WriteConfig:
                        taskList.Add(new Task(async () => await ConfigManager.WriteConfigFileAsync()));
                        break;
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