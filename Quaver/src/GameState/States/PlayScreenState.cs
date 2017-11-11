using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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
using Quaver.Input;
using Quaver.Logging;
using Quaver.Main;
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
        private TextSprite SVText { get; set; }

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
            Console.WriteLine($"[GAMEPLAY STATE] Initialized Gameplay State with Mods: { String.Join(", ", GameBase.CurrentGameModifiers.Select(x => x.ModIdentifier)) }");
            Console.WriteLine($"[GAMEPLAY STATE] Loaded Beatmap: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} [{GameBase.SelectedBeatmap.DifficultyName}]");
            Console.WriteLine($"[GAMEPLAY STATE] Loaded Beatmap MD5: {BeatmapMd5}");
            Console.WriteLine($"[GAMEPLAY STATE] Beatmap has Key Count: {GameBase.SelectedBeatmap.Qua.KeyCount}");

            GameBase.DiscordController.presence.details = $"Playing: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} ({GameBase.SelectedBeatmap.DifficultyName})";
            DiscordRPC.UpdatePresence(ref GameBase.DiscordController.presence);

            //Create loggers
            LogManager.AddLogger("KeyCount", Color.Pink);
            LogManager.AddLogger("DeltaTime", Color.LawnGreen);
            LogManager.AddLogger("SongTime", Color.White);
            LogManager.AddLogger("SongPos", Color.White);
            LogManager.AddLogger("HitObjects", Color.Wheat);
            LogManager.AddLogger("Skippable", CustomColors.NameTagAdmin);

            //Initialize Components
            Playfield.InitializePlayfield();
            Timing.InitializeTiming(GameBase.SelectedBeatmap.Qua);
            NoteRendering.InitializeNotes(GameBase.SelectedBeatmap.Qua);

            //Todo: Remove. TEST.
            TestButton = new TextButton(new Vector2(200, 50), "BACK")
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
            SVText = new TextSprite()
            {
                Size = new Vector2(240,190),
                Position = new Vector2(5,5),
                Alignment = Alignment.TopLeft,
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
        public void LoadContent() { }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void UnloadContent()
        {
            //Remove Loggers
            LogManager.RemoveLogger("KeyCount");
            LogManager.RemoveLogger("DeltaTime");
            LogManager.RemoveLogger("SongTime");
            LogManager.RemoveLogger("SongPos");
            LogManager.RemoveLogger("HitObjects");
            LogManager.RemoveLogger("Skippable");

            //Do Unload stuff
            TestButton.Clicked -= ButtonClick;
            NoteRendering.Boundary.Destroy();
            Playfield.Boundary.Destroy();

            //todo: temp
            TextUnder.Destroy();

            UpdateReady = false;
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Get the current game time in milliseconds.
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Set the current song time.
            Timing.SetCurrentSongTime(dt);

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
            LogManager.UpdateLogger("DeltaTime", "Delta Time: " + dt + "ms");
            LogManager.UpdateLogger("SongTime", "Current Song Time: " + Timing.CurrentSongTime + "ms");
            LogManager.UpdateLogger("SongPos", "Current Track Position: " + NoteRendering.TrackPosition);
            LogManager.UpdateLogger("HitObjects", "Total Remaining Notes: " + NoteRendering.HitObjectPool.Count);
            LogManager.UpdateLogger("Skippable", $"Intro Skippable: {IntroSkippable}");

            //Todo: remove. TEST.
            TextUnder.Update(dt);
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Draw()
        {
            Playfield.Boundary.Draw();
            NoteRendering.Boundary.Draw();
            TestButton.Draw();
            TextUnder.Draw();
        }

        public void ButtonClick(object sender, EventArgs e)
        {
            GameStateManager.Instance.ChangeState(new SongSelectState());
        }
    }
}