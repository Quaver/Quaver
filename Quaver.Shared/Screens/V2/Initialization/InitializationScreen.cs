using System;
using System.IO;
using System.Linq;
using System.Threading;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Steamworks;
using Wobble;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Scheduling;

namespace Quaver.Shared.Screens.V2.Initialization
{
    /// <summary>
    ///     V2 startup screen. It prepares V2 UI prerequisites before constructing its view,
    ///     then performs the same game initialization work as the legacy startup screen.
    /// </summary>
    internal sealed class InitializationScreen : PlaceholderScreen
    {
        private static readonly TimeSpan RemoveBackupInterval = TimeSpan.FromDays(2);

        public override QuaverScreenType Type { get; } = QuaverScreenType.Initialization;

        protected override bool UsesNavigation => false;

        protected override bool CanExitToMainMenu => false;

        private TaskHandler<int, int> InitializationTask { get; }

        public InitializationScreen()
        {
            Logger.Important("Loading skin...", LogType.Runtime);
            SkinManager.Load(UniversalSkinElementsLoadFlags.All &
                             ~UniversalSkinElementsLoadFlags.SoundEffects);

            Logger.Important("Loading fonts...", LogType.Runtime);
            Fonts.LoadWobbleFonts();

            InitializationTask = new TaskHandler<int, int>(RunInitializationTask);
            InitializationTask.OnCompleted += OnInitializationComplete;
            View = new InitializationScreenView(this);
        }

        public override void OnFirstUpdate()
        {
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 0;
            Button.IsGloballyClickable = false;
            InitializationTask.Run(0);
        }

        public override void Destroy()
        {
            InitializationTask.OnCompleted -= OnInitializationComplete;
            InitializationTask.Dispose();
            base.Destroy();
        }

        public override UserClientStatus GetClientStatus() => null;

        private static int RunInitializationTask(int value, CancellationToken token)
        {
            Logger.Important("Performing game initialization task...", LogType.Runtime);
            token.ThrowIfCancellationRequested();

            var game = (QuaverGame) GameBase.Game;
            game.SetProcessPriority();
            game.PerformGameSetup();

            token.ThrowIfCancellationRequested();
            if (SteamManager.IsInitialized)
                SteamManager.SendAvatarRetrievalRequest(SteamUser.GetSteamID().m_SteamID);

            BackgroundHelper.Initialize();
            game.CreateFpsCounter();
            BackgroundManager.Initialize();
            Transitioner.Initialize();
            return value;
        }

        private static void OnInitializationComplete(object sender, TaskCompleteEventArgs<int, int> args)
        {
            Logger.Important("Game initialization task complete!", LogType.Runtime);
            new Thread(CleanOldMapBackups).Start();
#if !VISUAL_TESTS
            QuaverScreenManager.ScheduleScreenChange(() => QuaverScreenFactory.CreateMainMenu());
#endif
        }

        private static void CleanOldMapBackups()
        {
            Directory.CreateDirectory(ConfigManager.MapBackupDirectory);
            var deleted = 0;
            var kept = 0;

            foreach (var path in Directory.GetFiles(ConfigManager.MapBackupDirectory, "*.qua",
                         SearchOption.AllDirectories))
            {
                if (!DateTime.TryParse(Path.GetFileNameWithoutExtension(path).Replace('_', ':'), out var time) ||
                    DateTime.Now - time <= RemoveBackupInterval)
                {
                    kept++;
                    continue;
                }

                deleted++;
                File.Delete(path);
            }

            foreach (var directory in Directory.GetDirectories(ConfigManager.MapBackupDirectory)
                         .Where(path => !Directory.EnumerateFiles(path).Any()))
                Directory.Delete(directory);

            Logger.Important($"Removed {deleted} map backup(s) while keeping {kept}.", LogType.Runtime);
        }
    }
}
