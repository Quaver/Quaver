using System;
using System.IO;
using System.Linq;
using System.Threading;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Skinning;
using Steamworks;
using Wobble;
using Wobble.Logging;
using Wobble.Scheduling;

namespace Quaver.Shared.Screens.Initialization
{
    public sealed class InitializationScreen : QuaverScreen
    {
        static readonly TimeSpan _removeBackupInterval = TimeSpan.FromDays(2);

        public override QuaverScreenType Type { get; } = QuaverScreenType.Initialization;

        private TaskHandler<int, int> InitializationTask { get; }

        public InitializationScreen()
        {
            InitializationTask = new TaskHandler<int, int>(RunInitializationTask);
            InitializationTask.OnCompleted += OnInitializationComplete;

            View = new InitializationScreenView(this);
        }

        public override void OnFirstUpdate()
        {
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 0;

            Logger.Important($"Loading skin...", LogType.Runtime);
            SkinManager.Load();

            Logger.Important($"Loading fonts...", LogType.Runtime);
            Fonts.LoadWobbleFonts();

            InitializationTask.Run(0);

            base.OnFirstUpdate();
        }

        public override void Destroy()
        {
            InitializationTask.Dispose();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        private int RunInitializationTask(int arg1, CancellationToken arg2)
        {
            Logger.Important($"Performing game initialization task... ", LogType.Runtime);

            var game = (QuaverGame) GameBase.Game;

            game.SetProcessPriority();
            game.PerformGameSetup();

            SteamManager.SendAvatarRetrievalRequest(SteamUser.GetSteamID().m_SteamID);
            BackgroundHelper.Initialize();

            // Create the global FPS counter.
            game.CreateFpsCounter();
            BackgroundManager.Initialize();
            Transitioner.Initialize();

            return 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInitializationComplete(object sender, TaskCompleteEventArgs<int, int> e)
        {
            Logger.Important($"Game initialization task complete!", LogType.Runtime);

            new Thread(CleanOldMapBackups).Start();
#if !VISUAL_TESTS
            QuaverScreenManager.ScheduleScreenChange(() => new MainMenuScreen());
#endif
        }

        private static void CleanOldMapBackups()
        {
            Logger.Important("Removing old map backups...", LogType.Runtime);
            Directory.CreateDirectory(ConfigManager.MapBackupDirectory);
            var deleted = 0;
            var kept = 0;

            foreach (var path in Directory.GetFiles(ConfigManager.MapBackupDirectory, "*.qua", SearchOption.AllDirectories))
            {
                if (!DateTime.TryParse(Path.GetFileNameWithoutExtension(path).Replace('_', ':'), out var time) ||
                    DateTime.Now - time <= _removeBackupInterval)
                {
                    kept++;
                    continue;
                }

                deleted++;
                File.Delete(path);
            }

            var emptyDirectories = Directory
               .GetDirectories(ConfigManager.MapBackupDirectory)
               .Where(path => !Directory.EnumerateFiles(path).Any());

            foreach (var directory in emptyDirectories)
                Directory.Delete(directory);

            Logger.Important($"Removed {deleted} map backup(s) while keeping {kept}.", LogType.Runtime);
        }

        public override UserClientStatus GetClientStatus() => null;
    }
}
