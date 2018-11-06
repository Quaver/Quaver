using System;
using System.Threading;
using Amib.Threading;
using Quaver.Graphics.Notifications;
using Wobble.Logging;
using Action = Amib.Threading.Action;

namespace Quaver.Scheduling
{
    public static class ThreadScheduler
    {
        /// <summary>
        ///     Thread pool used to run things in the background.
        /// </summary>
        public static readonly SmartThreadPool ThreadPool = new SmartThreadPool(new STPStartInfo
        {
            AreThreadsBackground = true,
            IdleTimeout = 600000,
            MaxWorkerThreads = 32,
            MinWorkerThreads = 8
        });

        /// <summary>
        ///     Runs in the background.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IWorkItemResult Run(Action action) => ThreadPool.QueueWorkItem(delegate
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, "Error occurred while running background thread. Check runtime.log!");
            }
        });

        /// <summary>
        ///     Runs thread after time.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static IWorkItemResult RunAfter(Action action, int time) => ThreadPool.QueueWorkItem(delegate
        {
            Thread.Sleep(time);

            try
            {
                action();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, "Error occurred while running background thread. Check runtime.log!");
            }
        });
    }
}