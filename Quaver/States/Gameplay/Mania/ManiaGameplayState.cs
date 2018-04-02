using Quaver.API.Maps;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Logging;
using Quaver.Main;
using Quaver.States.Enums;

namespace Quaver.States.Gameplay.Mania
{
    /// <summary>
    ///     This is the GameState when the player is actively playing.
    /// </summary>
    internal class ManiaGameplayState : IGameState
    {
        public State CurrentState { get; set; } = State.PlayScreen;
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Note Manager
        /// </summary>
        private ManiaGameplayManager ManiaGameplayManager { get; set; }

        /// <summary>
        ///     Constructor, data passed in from loading state
        /// </summary>
        /// <param name="beatmapMd5"></param>
        public ManiaGameplayState(Qua qua, string beatmapMd5)
        {
            ManiaGameplayManager = new ManiaGameplayManager(qua, beatmapMd5);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Initialize()
        {
            // Initialize Note Manager
            ManiaGameplayManager.Initialize(this);

            // Update window title
            GameBase.GameWindow.Title = $"Quaver - {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} [{GameBase.SelectedBeatmap.DifficultyName}]";

            // Update Discord Presence
            DiscordController.ChangeDiscordPresenceGameplay(false);

            UpdateReady = true;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void UnloadContent()
        {
            UpdateReady = false;
            ManiaGameplayManager.UnloadContent();

            //Remove Loggers
            Logger.Clear();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Update(double dt)
        {
            ManiaGameplayManager.Update(dt);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Draw()
        {
            ManiaGameplayManager.Draw();
        }
    }
}
