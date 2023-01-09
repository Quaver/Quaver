using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Beta;
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

#if !VISUAL_TESTS
            QuaverScreenManager.ScheduleScreenChange(() => new MainMenuScreen());
#endif
        }

        public override UserClientStatus GetClientStatus() => null;
    }
}