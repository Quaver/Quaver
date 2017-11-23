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
using Quaver.Gameplay;
using Quaver.Gameplay.GameplayRendering;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Button;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using Quaver.Input;
using Quaver.Logging;
using Quaver.Replays;

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

        public GameplayUI GameplayUI;
        public NoteManager NoteManager;
        public NoteRendering NoteRendering;
        public Playfield Playfield;
        public ScoreManager ScoreManager;
        public Timing Timing;

        //todo: remove. TEST.
        private Sprite TextUnder { get; set; }
        private TextBoxSprite SVText { get; set; }

        /// <summary>
        ///     The input manager for this game state.
        /// </summary>
        private GameplayInputManager InputManager { get; set; }

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
        ///     Holds the list of replay frames for this state.
        /// </summary>
        private List<ReplayFrame> ReplayFrames { get; set;}

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
            // Create Gameplay classes
            GameplayUI = new GameplayUI();
            NoteManager = new NoteManager();
            NoteRendering = new NoteRendering();
            Playfield = new Playfield();
            ScoreManager = new ScoreManager();
            Timing = new Timing();
            InputManager = new GameplayInputManager(NoteManager);
            ReplayFrames = new List<ReplayFrame>();

            // Update window title
            GameBase.GameWindow.Title = $"Quaver - {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title} [{GameBase.SelectedBeatmap.DifficultyName}]";

            // Update Discord Presence
            GameBase.ChangeDiscordPresenceGameplay(false);

            // Create the file for replays if debugging
            if (Configuration.Debug)
                File.Create(ReplayHelper.DebugFilePath);
            
            //Todo: remove
            Logger.Add("KeyCount", "", Color.Pink);
            Logger.Add("SongPos", "", Color.White);
            Logger.Add("Skippable", "", CustomColors.NameTagAdmin);
            Logger.Add("JudgeDifficulty", "", CustomColors.NameTagModerator);

            // Initialize Gameplay
            InitializeGameplay();

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
            GameplayUI.UnloadContent();

            //Remove Loggers
            Logger.Clear();

            //Destroy boundarys
            TestButton.Clicked -= ButtonClick;

            //todo: temp
            TextUnder.Destroy();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Update(double dt)
        {
            // Set the current song time.
            Timing.Update(dt);

            // Check if the song is currently skippable.
            IntroSkippable = (GameBase.SelectedBeatmap.Qua.HitObjects[0].StartTime - Timing.CurrentSongTime >= 5000);

            // Update the playfield
            Playfield.Update(dt);

            // Update the Notes
            NoteRendering.Update(dt);

            // Update Data Interface
            GameplayUI.Update(dt);

            // Check the input for this particular game state.
            InputManager.CheckInput(GameBase.SelectedBeatmap.Qua, IntroSkippable, ReplayFrames);

            // Update Loggers. todo: remove
            Logger.Update("KeyCount", $"Key Count: {GameBase.SelectedBeatmap.Qua.KeyCount}");
            Logger.Update("SongPos", "Current Track Position: " + NoteRendering.TrackPosition);
            Logger.Update("Skippable", $"Intro Skippable: {IntroSkippable}");

            //Todo: remove. TEST.
            TextUnder.Update(dt);

            if (Timing.PlayingIsDone)
                GameBase.GameStateManager.ChangeState(new ScoreScreenState(BeatmapMd5, ScoreManager, GameBase.SelectedBeatmap.Artist, GameBase.SelectedBeatmap.Title, GameBase.SelectedBeatmap.DifficultyName));
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Draw()
        {
            Playfield.DrawUnder();
            NoteRendering.Draw();
            Playfield.DrawOver();
            GameplayUI.Draw();
            TestButton.Draw();
            //TextUnder.Draw();
        }

        public void ButtonClick(object sender, EventArgs e)
        {
            GameBase.GameStateManager.ChangeState(new SongSelectState());
        }

        /// <summary>
        ///     Solely responsible for intializing gameplay aspects
        /// </summary>
        private void InitializeGameplay()
        {
            //Initialize Score Manager
            //todo: temp
            var count = 0;
            var total = GameBase.SelectedBeatmap.Qua.HitObjects.Count;

            foreach (var ho in GameBase.SelectedBeatmap.Qua.HitObjects)
            {
                if (ho.EndTime > ho.StartTime) count++;
            }

            ScoreManager.Initialize(total + count, GameBase.SelectedBeatmap.Qua.Judge); //TODO: ADD RELEASE COUNTS AS WELL

            //Initialize the rest
            Playfield.Initialize(this);
            Timing.Initialize(this);
            NoteRendering.Initialize(this);
            GameplayUI.Initialize(this);
            NoteManager.Initialize(this);

            //Update logger. todo: remove
            var loggertext = "Hitwindow: Judge: " + ScoreManager.JudgeDifficulty + "   Press: ";
            foreach (var a in ScoreManager.HitWindowPress) loggertext += Math.Floor(a) + "ms, ";
            loggertext += "   Release: ";
            foreach (var a in ScoreManager.HitWindowRelease) loggertext += Math.Floor(a) + "ms, ";

            Logger.Update("JudgeDifficulty", loggertext);
        }
    }
}
