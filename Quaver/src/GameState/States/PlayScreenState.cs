using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using Quaver.Input;
using Quaver.Logging;
using Quaver.Replays;

using Quaver.GameState.Gameplay;
using Quaver.GameState.Gameplay.PlayScreen;
using Quaver.Modifiers;
using Quaver.QuaFile;
using Quaver.Utility;
using Button = Quaver.Graphics.Button.Button;

namespace Quaver.GameState.States
{
    /// <summary>
    ///     This is the GameState when the player is actively playing.
    /// </summary>
    internal class PlayScreenState : IGameState
    {
        public State CurrentState { get; set; } = State.PlayScreen;
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Note Manager
        /// </summary>
        private NoteManager NoteManager { get; set; }

        /// <summary>
        ///     Constructor, data passed in from loading state
        /// </summary>
        /// <param name="beatmapMd5"></param>
        public PlayScreenState(Qua qua, string beatmapMd5)
        {
            NoteManager = new NoteManager(qua, beatmapMd5);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Initialize()
        {
            // Initialize Note Manager
            NoteManager.Initialize(this);

            // Update window title
            GameBase.GameWindow.Title = $"Quaver - {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} [{GameBase.SelectedBeatmap.DifficultyName}]";

            // Update Discord Presence
            GameBase.ChangeDiscordPresenceGameplay(false);

            UpdateReady = true;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void UnloadContent()
        {
            UpdateReady = false;

            //Remove Loggers
            Logger.Clear();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Update(double dt)
        {
            NoteManager.Update(dt);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Draw()
        {
            NoteManager.Draw();
        }
    }
}
