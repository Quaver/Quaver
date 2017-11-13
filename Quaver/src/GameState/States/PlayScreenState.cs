﻿using System;
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
using Quaver.Gameplay;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using Quaver.Input;
using Quaver.Logging;

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

        //todo: remove. TEST.
        private Sprite TextUnder { get; set; }
        private TextBoxSprite SVText { get; set; }

        /// <summary>
        ///     The input manager for this game state.
        /// </summary>
        private GameplayInputManager InputManager { get; } = new GameplayInputManager();

        /// <summary>
        ///     The MD5 Hash of the played beatmap.
        /// </summary>
        private string BeatmapMd5 { get; set; }

        /// <summary>
        ///     Keeps track of whether or not the song intro is current skippable.
        /// </summary>
        private bool IntroSkippable { get; set; }

        //TODO:Remove later.   TEST
        private Button TestButton { get; set; }

        /// <summary>
        ///     Ctor, data passed in from loading state
        /// </summary>
        /// <param name="qua"></param>
        /// <param name="beatmapMd5"></param>
        public PlayScreenState(string beatmapMd5)
        {
            BeatmapMd5 = beatmapMd5;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Initialize()
        {
            // Update Discord Presence
            HandleDiscordPresence(false);

            //Todo: Remove. Create loggers
            LogManager.AddLogger("KeyCount", Color.Pink);
            LogManager.AddLogger("SongPos", Color.White);
            LogManager.AddLogger("Skippable", CustomColors.NameTagAdmin);

            for (var i=0; i < NoteManager.TimingNames.Length; i++)
            {
                LogManager.AddLogger(NoteManager.TimingNames[i], NoteManager.TimingColors[i]);
            }

            //Todo: Remove. TEST.
            TestButton = new TextButton(new Vector2(200, 30), "BACK")
            {
                Image = GameBase.LoadedSkin.ColumnTimingBar,
                Alignment = Alignment.TopCenter,
                Parent = NoteRendering.Boundary
            };
            TestButton.Clicked += ButtonClick;

            TextUnder = new Sprite()
            {
                Image = GameBase.UI.HollowBox,
                Tint = Color.Blue,
                Size = new Vector2(250,200),
                Alignment = Alignment.TopRight
            };

            var temp = "SV Points: ";
            foreach (var sv in Timing.SvQueue) temp += "[" + sv.TargetTime + ", " + sv.SvMultiplier + "x], ";
            SVText = new TextBoxSprite()
            {
                Size = new Vector2(240,190),
                Position = new Vector2(5,5),
                TextAlignment = Alignment.TopLeft,
                Parent = TextUnder,
                TextColor = Color.Yellow,
                Multiline = true,
                Textwrap = true,
                Text = temp
            };

            UpdateReady = true;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void UnloadContent()
        {
            UpdateReady = false;

            //Unload Content from other classes
            NoteRendering.UnloadContent();
            Timing.UnloadContent();
            Playfield.UnloadContent();

            //Remove Loggers
            LogManager.ClearLogger();

            //Destroy boundarys
            TestButton.Clicked -= ButtonClick;

            //todo: temp
            TextUnder.Destroy();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Get the current game time in milliseconds.
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Set the current song time.
            Timing.Update(dt);

            // Check if the song is currently skippable.
            IntroSkippable = (GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - Timing.CurrentSongTime >= 5000);

            // Update the playfield
            Playfield.Update(dt); ;

            // Update the Notes
            NoteRendering.Update(dt);

            // Check the input for this particular game state.
            InputManager.CheckInput(GameBase.SelectedBeatmap.Qua, IntroSkippable);

            // Update Loggers
            LogManager.UpdateLogger("KeyCount", $"Key Count: {GameBase.SelectedBeatmap.Qua.KeyCount}");
            LogManager.UpdateLogger("SongPos", "Current Track Position: " + NoteRendering.TrackPosition);
            LogManager.UpdateLogger("Skippable", $"Intro Skippable: {IntroSkippable}");

            //Todo: remove. TEST.
            TextUnder.Update(dt);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Draw()
        {
            Playfield.DrawUnder();
            NoteRendering.Draw();
            Playfield.DrawOver();
            TestButton.Draw();
            TextUnder.Draw();
        }

        public void ButtonClick(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Responsible for handling discord presence w/ mods if any exist.
        /// </summary>
        public static void HandleDiscordPresence(bool skippedSong)
        {
            var sb = new StringBuilder();
            sb.Append($"Playing: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} [{GameBase.SelectedBeatmap.DifficultyName}]");

            // Get original song length
            var songLength = GameBase.SelectedBeatmap.Song.GetAudioLength() / GameBase.GameClock;
            
            // Get the new time in the song if it was skipped.
            if (skippedSong)
                songLength = (GameBase.SelectedBeatmap.Song.GetAudioLength() - GameBase.SelectedBeatmap.Song.GetAudioPosition()) / GameBase.GameClock;

            // Add mods to the string if mods exist
            if (GameBase.CurrentGameModifiers.Count > 0)
            {
                sb.Append(" with mods: ");

                if (GameBase.CurrentGameModifiers.Exists(x => x.ModIdentifier == ModIdentifier.Speed))
                    sb.Append($"Speed {GameBase.GameClock}x");
            }

            GameBase.ChangeDiscordPresence(sb.ToString(), songLength);
        }
    }
}