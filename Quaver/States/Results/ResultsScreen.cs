using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Main;
using Quaver.States.Gameplay;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Discord;
using Quaver.Graphics.Text;
using Quaver.Graphics.UI;
using Quaver.Graphics.UI.Notifications;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.States.Gameplay.Replays;
using Quaver.States.Results.Input;
using Quaver.States.Results.UI;
using Quaver.States.Select;
using AudioEngine = Quaver.Audio.AudioEngine;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.States.Results
{
    internal class ResultsScreen : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public State CurrentState { get; set; } = State.Results;

        /// <summary>
        ///     The type of results screen.
        /// </summary>
        private ResultsScreenType Type { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     Reference to the gameplay screen that was just played.
        /// </summary>
        internal GameplayScreen GameplayScreen { get; }

        /// <summary>
        ///     All the UI elements.
        /// </summary>
        internal ResultsInterface UI { get; private set; }

        /// <summary>
        ///     The .qua that this is results screen is referencing to.
        /// </summary>
        internal Qua Qua { get; private set; }

        /// <summary>
        ///     Applause sound effect.
        /// </summary>
        private SoundEffectInstance ApplauseSound { get; set; }

        /// <summary>
        ///     Song title + Difficulty name.
        /// </summary>
        private string SongTitle => $"{Qua.Artist} - {Qua.Title} [{Qua.DifficultyName}]";

        /// <summary>
        ///     MD5 Hash of the map played.
        /// </summary>
        private string Md5 => GameplayScreen.MapHash;

        /// <summary>
        ///     The user's scroll speed.
        /// </summary>
        private int ScrollSpeed => Qua.Mode == GameMode.Keys4 ? ConfigManager.ScrollSpeed4K.Value : ConfigManager.ScrollSpeed7K.Value;

        /// <summary>
        ///     The replay that was just played.
        /// </summary>
        internal Replay Replay { get; private set; }

        /// <summary>
        ///     Score processor.
        /// </summary>
        internal ScoreProcessor ScoreProcessor { get; private set; }

        /// <summary>
        ///     Handles all input for this screen.
        /// </summary>
        private ResultsInputManager InputManager { get; set;  }

        /// <summary>
        ///     Boolean value that dictates if the screen is currently exiting.
        /// </summary>
        internal bool IsExiting => OnExit != null;

        /// <summary>
        ///     If the screen is currently exiting.
        /// </summary>
        private Action OnExit { get; set; }

        /// <summary>
        ///     The amount of time since the screen's exit was initiated.
        /// </summary>
        private double TimeSinceScreenExiting { get; set; }

        /// <summary>
        ///     If the screen's exit action has already been invoked.
        /// </summary>
        private bool ExitInvoked { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="gameplay"></param>
        public ResultsScreen(GameplayScreen gameplay)
        {
            GameplayScreen = gameplay;
            Qua = GameplayScreen.Map;
            Type = ResultsScreenType.FromGameplay;
        }

        /// <summary>
        ///     When going to the results screen with just a replay.
        /// </summary>
        /// <param name="replay"></param>
        public ResultsScreen(Replay replay)
        {
            Replay = replay;
            ScoreProcessor = new ScoreProcessorKeys(Replay);
            Type = ResultsScreenType.FromReplayFile;
        }

        /// <summary>
        ///     When loading up the results screen with a local score.
        /// </summary>
        /// <param name="score"></param>
        public ResultsScreen(LocalScore score)
        {
            GameBase.SelectedMap.Qua = GameBase.SelectedMap.LoadQua();
            Qua = GameBase.SelectedMap.Qua;

            var localPath = $"{ConfigManager.DataDirectory.Value}/r/{score.Id}.qr";

            // Try to find replay w/ local score id.
            // Otherwise we want to find
            if (File.Exists(localPath))
            {
                Replay = new Replay(localPath);
            }
            // Otherwise we want to create an "artificial" replay with the local score data..
            else
            {
                Replay = new Replay(score.Mode, score.Name, score.Mods, score.MapMd5)
                {
                    Date = Convert.ToDateTime(score.DateTime, CultureInfo.InvariantCulture),
                    Score = score.Score,
                    Accuracy = (float) score.Accuracy,
                    MaxCombo = score.MaxCombo,
                    CountMarv = score.CountMarv,
                    CountPerf = score.CountPerf,
                    CountGreat = score.CountGreat,
                    CountGood = score.CountGood,
                    CountOkay = score.CountOkay,
                    CountMiss = score.CountMiss
                };
            }

            ScoreProcessor = new ScoreProcessorKeys(Replay);
            Type = ResultsScreenType.FromLocalScore;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {
            // Initialize the state depending on if we're coming from the gameplay screen
            // or loading up a replay file.
            switch (Type)
            {
                case ResultsScreenType.FromGameplay:
                    InitializeFromGameplay();
                    break;
                case ResultsScreenType.FromReplayFile:
                    InitializeFromReplayFile();
                    break;
                case ResultsScreenType.FromLocalScore:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Create the actual interface.
            UI = new ResultsInterface(this);
            UI.Initialize(this);

            InputManager = new ResultsInputManager(this);

            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UnloadContent()
        {
            UI.UnloadContent();
            OnExit = null;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            GameBase.Navbar.PerformHideAnimation(dt);
            GameBase.Cursor.Alpha = 1;

            InputManager.HandleInput(dt);
            HandleExit(dt);

            UI.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Black);
            UI.Draw();
        }

        /// <summary>
        ///     Changes discord rich presence to show results.
        /// </summary>
        private void ChangeDiscordPresence()
        {
            DiscordManager.Presence.Timestamps = null;

            // Don't change if we're loading in from a replay file.
            if (Type == ResultsScreenType.FromReplayFile || GameplayScreen.InReplayMode)
            {
                DiscordManager.Presence.Details = "Idle";
                DiscordManager.Presence.State = "In the menus";
                DiscordManager.Client.SetPresence(DiscordManager.Presence);
                return;
            }

            var state = GameplayScreen.Failed ? "Fail" : "Pass";
            var score = $"{ScoreProcessor.Score / 1000}k";
            var acc = $"{StringHelper.AccuracyToString(ScoreProcessor.Accuracy)}";
            var grade = GameplayScreen.Failed ? "F" : GradeHelper.GetGradeFromAccuracy(ScoreProcessor.Accuracy).ToString();
            var combo = $"{ScoreProcessor.MaxCombo}x";

            DiscordManager.Presence.State = $"{state}: {grade} {score} {acc} {combo}";
            DiscordManager.Client.SetPresence(DiscordManager.Presence);
        }

        /// <summary>
        ///     Plays the appluase sound effect.
        /// </summary>
        private void PlayApplauseEffect()
        {
            ApplauseSound = GameBase.Skin.SoundApplause.CreateInstance();

            if (!GameplayScreen.Failed && ScoreProcessor.Accuracy >= 80 && !GameplayScreen.InReplayMode)
                ApplauseSound.Play();
        }

        /// <summary>
        ///     Goes through the score submission process.
        /// </summary>
        private void SubmitScore()
        {
            // Don't save scores if the user quit themself.
            if (GameplayScreen.HasQuit || GameplayScreen.InReplayMode)
                return;

            // Run all of these tasks inside of a new thread to avoid blocks.
            Task.Run(() => { SaveLocalScore(); });

/*#if DEBUG
            Task.Run(() => { SaveDebugReplayData(); });
#endif*/
        }

        /// <summary>
        ///     Initializes the results screen if we're coming from the gameplay screen.
        /// </summary>
        private void InitializeFromGameplay()
        {
            // Keep the same replay and score processor if the user was watching a replay before.
            if (GameplayScreen.InReplayMode)
            {
                Replay = GameplayScreen.LoadedReplay;
                ScoreProcessor = Replay.Mods.HasFlag(ModIdentifier.Autoplay) ? GameplayScreen.Ruleset.ScoreProcessor : new ScoreProcessorKeys(Replay);
            }
            // Otherwise the replay and processor should be the one that the user just played.
            else
            {
                // Populate the replay with values from the score processor.
                Replay = GameplayScreen.ReplayCapturer.Replay;
                ScoreProcessor = GameplayScreen.Ruleset.ScoreProcessor;

                Replay.FromScoreProcessor(ScoreProcessor);
            }

            ChangeDiscordPresence();
            PlayApplauseEffect();

            // Submit score
            SubmitScore();
        }

        /// <summary>
        ///     Initialize the screen if we're coming from a replay file.
        /// </summary>
        private void InitializeFromReplayFile()
        {
            var mapset = GameBase.Mapsets.FirstOrDefault(x => x.Maps.Any(y => y.Md5Checksum == Replay.MapMd5));

            // Send the user back to the song select screen with an error if there was no found mapset.
            if (mapset == null)
            {
                Logger.LogError($"You do not have the map that this replay is for", LogType.Runtime);
                GameBase.GameStateManager.ChangeState(new SongSelectState());
                return;
            }

            // Find the map that actually has the correct hash.
            var map = mapset.Maps.Find(x => x.Md5Checksum == Replay.MapMd5);
            Map.ChangeSelected(map);

            // Load up the .qua file and change the selected map's Qua.
            Qua = map.LoadQua();
            GameBase.SelectedMap.Qua = Qua;

            // Make sure the background is loaded, we don't run this async because we
            // want it to be loaded when the user starts the map.
            BackgroundManager.LoadBackground();
            BackgroundManager.Change(GameBase.CurrentBackground);

            // Reload and play song.
            try
            {
                GameBase.AudioEngine.ReloadStream();
                GameBase.AudioEngine.Play();
            }
            catch (AudioEngineException e)
            {
                // No need to handle here.
            }
        }

        /// <summary>
        ///     Saves a local score to the database.
        /// </summary>
        private void SaveLocalScore()
        {
            var scoreId = 0;
            try
            {
                var localScore = LocalScore.FromScoreProcessor(ScoreProcessor, Md5, ConfigManager.Username.Value, ScrollSpeed);
                scoreId = LocalScoreCache.InsertScoreIntoDatabase(localScore);
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "There was an error saving your score. Check Runtime.log for more details.");
                Logger.LogError(e, LogType.Runtime);
            }

            try
            {
                Replay.Write($"{ConfigManager.DataDirectory}/r/{scoreId}.qr");
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "There was an error when saving your replay. Check Runtime.log for more details.");
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Saves replay data related to debugging.
        /// </summary>
        private void SaveDebugReplayData()
        {
            try
            {
                File.WriteAllText($"{ConfigManager.DataDirectory.Value}/replay_debug.txt", Replay.FramesToString(true));

                var hitStats = "";
                GameplayScreen.Ruleset.ScoreProcessor.Stats.ForEach(x => hitStats += $"{x.ToString()}\r\n");
                File.WriteAllText($"{ConfigManager.DataDirectory.Value}/replay_debug_hitstats.txt", hitStats);
            }
            catch (Exception e)
            {
                Logger.LogError($"There was an error when writing debug replay files: {e}", LogType.Runtime);
            }
        }

        /// <summary>
        ///     Exports the currently looked at replay.
        /// </summary>
        internal void ExportReplay()
        {
            NotificationManager.Show(NotificationLevel.Info, "One moment, we're exporting your replay.");

            if (!Replay.HasData)
            {
                NotificationManager.Show(NotificationLevel.Error, "This replay doesn't have any data!");
                return;
            }

            if (Replay.Mods.HasFlag(ModIdentifier.Autoplay))
            {
                NotificationManager.Show(NotificationLevel.Error, "Exporting autoplay replays is disabled.");;
                return;
            }

            Task.Run(() =>
            {
                var path = $@"{ConfigManager.ReplayDirectory.Value}/{Replay.PlayerName} - {StringHelper.FileNameSafeString(SongTitle)} - {DateTime.Now:yyyyddMMhhmmss}{GameBase.GameTime.ElapsedMilliseconds}.qr";
                Replay.Write(path);

                // Open containing folder
                Process.Start("explorer.exe", "/select, \"" + path.Replace("/", "\\") + "\"");

                NotificationManager.Show(NotificationLevel.Success, "The replay has been successfully exported!");
            });
        }

        /// <summary>
        ///     Initiates the screen exit process.
        /// </summary>
        internal void Exit(Action onExitScreen)
        {
            UI.ButtonContainer.MakeButtonsUnclickable();

            // Make sure this is only set one time in its existence.
            if (IsExiting)
                return;

            OnExit += onExitScreen;
        }

        /// <summary>
        ///     Counts the amount of time the screen has been in progress to exit, then does so accordinly.
        /// </summary>
        /// <param name="dt"></param>
        private void HandleExit(double dt)
        {
            if (!IsExiting)
                return;

            TimeSinceScreenExiting += dt;

            // The amount of time it takes for the screen to exit.
            const int screenExitTime = 800;

            if (TimeSinceScreenExiting >= screenExitTime && !ExitInvoked)
            {
                OnExit?.Invoke();
                ExitInvoked = true;
            }
        }

        /// <summary>
        ///     Action that goes back to the song select screen.
        /// </summary>
        internal void GoBackToMenu() => GameBase.GameStateManager.ChangeState(new SongSelectState());

        /// <summary>
        ///     Loads up local scores and watches the replay.
        /// </summary>
        internal void WatchReplay()
        {
            var scores = LocalScoreCache.FetchMapScores(GameBase.SelectedMap.Md5Checksum);
            GameBase.GameStateManager.ChangeState(new GameplayScreen(Qua, GameBase.SelectedMap.Md5Checksum, scores, Replay));
        }

        /// <summary>
        ///     Goes to the gameplay screen toplay the map.
        /// </summary>
        internal void RetryMap()
        {
            var scores = LocalScoreCache.FetchMapScores(GameBase.SelectedMap.Md5Checksum);
            GameBase.GameStateManager.ChangeState(new GameplayScreen(Qua, GameBase.SelectedMap.Md5Checksum, scores));
        }
    }
}