using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Graphics.Notifications;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.Scheduling;
using Quaver.Screens.Gameplay;
using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Screens.Menu;
using Quaver.Screens.Results.Input;
using Quaver.Screens.Select;
using Wobble;
using Wobble.Audio;
using Wobble.Discord;
using Wobble.Screens;

namespace Quaver.Screens.Results
{
    public class ResultsScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     Where we are coming from when loading this results screen.
        /// </summary>
        public ResultsScreenType Type { get; }

        /// <summary>
        ///     Reference to the gameplay screen that was just played (if it exists)
        /// </summary>
        public GameplayScreen GameplayScreen { get; }

        /// <summary>
        ///     Reference to the map that was played. (or will be played depending on the screen).
        /// </summary>
        public Qua Qua { get; private set; }

        /// <summary>
        ///     The replay that this results screen references to.
        ///     It can either be we're loading the screen directly from a replay so we're about to watch it,
        ///     or it can reference the replay that was just played from gameplay.
        /// </summary>
        public Replay Replay { get; private set; }

        /// <summary>
        ///     Contains score data information that'll be displayed to the user.
        /// </summary>
        public ScoreProcessor ScoreProcessor { get; private set; }

        /// <summary>
        ///     The local replay path if it exists.
        /// </summary>
        public string LocalReplayPath { get; private set; }

        /// <summary>
        ///     The user's scroll speed.
        /// </summary>
        private int ScrollSpeed => Qua.Mode == GameMode.Keys4 ? ConfigManager.ScrollSpeed4K.Value : ConfigManager.ScrollSpeed7K.Value;

        /// <summary>
        ///     Song title + Difficulty name.
        /// </summary>
        public string SongTitle => $"{Qua.Artist} - {Qua.Title} [{Qua.DifficultyName}]";

        /// <summary>
        ///     Boolean value that dictates if the screen is currently exiting.
        /// </summary>
        public bool IsExiting => OnExit != null;

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
        ///     Handles all input for this screen.
        /// </summary>
        private ResultsInputManager InputManager { get; set; }

        /// <summary>
        ///     Creating the results screen from gameplay.
        /// </summary>
        public ResultsScreen(GameplayScreen gameplayScreen)
        {
            GameplayScreen = gameplayScreen;
            Qua = GameplayScreen.Map;
            Type = ResultsScreenType.FromGameplay;

            InitializeScreen();
        }

        /// <summary>
        ///     Creates the results screen from loading up a replay.
        /// </summary>
        /// <param name="replay"></param>
        public ResultsScreen(Replay replay)
        {
            Replay = replay;
            ScoreProcessor = new ScoreProcessorKeys(replay);
            Type = ResultsScreenType.FromReplayFile;

            InitializeScreen();
        }

        /// <summary>
        ///     When loading up the results screen with a local score.
        /// </summary>
        /// <param name="score"></param>
        public ResultsScreen(LocalScore score)
        {
            MapManager.Selected.Value.Qua = MapManager.Selected.Value.LoadQua();
            Qua = MapManager.Selected.Value.Qua;

            LocalReplayPath = $"{ConfigManager.DataDirectory.Value}/r/{score.Id}.qr";

            Replay = new Replay(score.Mode, score.Name, score.Mods, score.MapMd5)
            {
                Date = Convert.ToDateTime(score.DateTime, CultureInfo.InvariantCulture),
                Score = score.Score,
                Accuracy = (float)score.Accuracy,
                MaxCombo = score.MaxCombo,
                CountMarv = score.CountMarv,
                CountPerf = score.CountPerf,
                CountGreat = score.CountGreat,
                CountGood = score.CountGood,
                CountOkay = score.CountOkay,
                CountMiss = score.CountMiss
            };

            ScoreProcessor = new ScoreProcessorKeys(Replay);
            Type = ResultsScreenType.FromLocalScore;

            InitializeScreen();
        }

        /// <summary>
        ///     Initializes the screen based on the type it is.
        /// </summary>
        private void InitializeScreen()
        {
            switch (Type)
            {
                case ResultsScreenType.FromGameplay:
                    InitializeFromGameplay();
                    ChangeDiscordPresence();
                    break;
                case ResultsScreenType.FromReplayFile:
                    InitializeFromReplayFile();
                    break;
                case ResultsScreenType.FromLocalScore:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            View = new ResultsScreenView(this);
            InputManager = new ResultsInputManager(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleExit(gameTime.ElapsedGameTime.TotalMilliseconds);
            InputManager.HandleInput();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnExit = null;
            base.Destroy();
        }

        /// <summary>
        ///     Goes through the score submission process.
        ///     saves score to local database.
        /// </summary>
        private void SubmitScore()
        {
            // Always clear cached scores after submitting
            MapManager.Selected.Value.ClearScores();

            // Don't save scores if the user quit themself.
            if (GameplayScreen.HasQuit || GameplayScreen.InReplayMode)
                return;

            Scheduler.RunThread(SaveLocalScore);

#if DEBUG
            Scheduler.RunThread(SaveDebugReplayData);
            Scheduler.RunThread(SaveHitData);
            Scheduler.RunThread(SaveHealthData);
#endif
        }

        /// <summary>
        ///     Initializes the results screen if we're coming from the gameplay screen.
        /// </summary>
        private void InitializeFromGameplay()
        {
            // Keep the same replay and score processor if the user was watching a replay before,
            if (GameplayScreen.InReplayMode)
            {
                Replay = GameplayScreen.LoadedReplay;
                ScoreProcessor = Replay.Mods.HasFlag(ModIdentifier.Autoplay) ? GameplayScreen.Ruleset.ScoreProcessor : new ScoreProcessorKeys(Replay);

                // Make sure the score processor's stats are up-to-date with the replay actually played.
                ScoreProcessor.Stats = GameplayScreen.Ruleset.ScoreProcessor.Stats;

                // Remove all the mods from the replay.
                ModManager.RemoveAllMods();
            }
            // Otherwise the replay and processor should be the one that the user just played.
            else
            {
                // Populate the replay with values from the score processor.
                Replay = GameplayScreen.ReplayCapturer.Replay;
                Replay.PauseCount = GameplayScreen.PauseCount;

                ScoreProcessor = GameplayScreen.Ruleset.ScoreProcessor;
                Replay.FromScoreProcessor(ScoreProcessor);

                // Remove paused modifier if enabled.
                if (ModManager.IsActivated(ModIdentifier.Paused))
                    ModManager.RemoveMod(ModIdentifier.Paused);
            }

            // Submit score
            SubmitScore();
        }

        /// <summary>
        ///     Initialize the screen if we're coming from a replay file.
        /// </summary>
        private void InitializeFromReplayFile()
        {
            var mapset = MapManager.Mapsets.FirstOrDefault(x => x.Maps.Any(y => y.Md5Checksum == Replay.MapMd5));

            // Send the user back to the song select screen with an error if there was no found mapset.
            if (mapset == null)
            {
                Logger.LogError($"You do not have the map that this replay is for", LogType.Runtime);
                ScreenManager.ChangeScreen(new MainMenuScreen());
                return;
            }

            // Find the map that actually has the correct hash.
            var map = mapset.Maps.Find(x => x.Md5Checksum == Replay.MapMd5);
            MapManager.Selected.Value = map;

            // Load up the .qua file and change the selected map's Qua.
            Qua = map.LoadQua();
            MapManager.Selected.Value.Qua = Qua;

            // TODO: Make sure background is loaded.

            // Reload and play song.
            try
            {
                AudioEngine.LoadCurrentTrack();
                AudioEngine.Track.Play();
            }
            catch (AudioEngineException)
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
                var localScore = LocalScore.FromScoreProcessor(ScoreProcessor, GameplayScreen.MapHash, ConfigManager.Username.Value, ScrollSpeed,
                    GameplayScreen.PauseCount);

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
        ///     Saves debug hit data to a local file.
        /// </summary>
        private void SaveHitData()
        {
            var str = "";

            for (var i = 0; i < ScoreProcessor.Stats.Count; i++)
            {
                var stat = ScoreProcessor.Stats[i];
                str += $"{i},{stat.HitDifference}\r\n";
            }

            File.WriteAllText(ConfigManager.DataDirectory + "/last_hit_data.txt", str);
        }

        /// <summary>
        ///     Saves debug hit data to a local file.
        /// </summary>
        private void SaveHealthData()
        {
            var str = "";

            for (var i = 0; i < ScoreProcessor.Stats.Count; i++)
            {
                var stat = ScoreProcessor.Stats[i];
                str += $"{i},{(int)stat.Health}\r\n";
            }

            File.WriteAllText(ConfigManager.DataDirectory + "/last_health_data.txt", str);
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
        public void ExportReplay()
        {
            NotificationManager.Show(NotificationLevel.Info, "One moment, we're exporting your replay.");

            if (Type == ResultsScreenType.FromLocalScore)
            {
                try
                {
                    Replay = new Replay(LocalReplayPath);
                }
                catch (Exception e)
                {
                    // ignored.
                }
            }

            if (!Replay.HasData)
            {
                NotificationManager.Show(NotificationLevel.Error, "This replay doesn't have any data!");
                return;
            }

            if (Replay.Mods.HasFlag(ModIdentifier.Autoplay))
            {
                NotificationManager.Show(NotificationLevel.Error, "Exporting autoplay replays is disabled.");
                return;
            }

            Task.Run(() =>
            {
                var path = $@"{ConfigManager.ReplayDirectory.Value}/{Replay.PlayerName} - {StringHelper.FileNameSafeString(SongTitle)} - {DateTime.Now:yyyyddMMhhmmss}{GameBase.Game.TimeRunning}.qr";
                Replay.Write(path);

                // Open containing folder
                Process.Start("explorer.exe", "/select, \"" + path.Replace("/", "\\") + "\"");

                NotificationManager.Show(NotificationLevel.Success, "The replay has been successfully exported!");
            });
        }

        /// <summary>
        ///     Initiates the screen exit process.
        /// </summary>
        public void Exit(Action onExitScreen)
        {
            // Make sure this is only set one time in its existence.
            if (IsExiting)
                return;

            var screenView = (ResultsScreenView)View;
            screenView.ButtonContainer.MakeButtonsUnclickable();

            OnExit += onExitScreen;

            // Fade the
            screenView.PerformExitAnimations();
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
        public void GoBackToMenu() => ScreenManager.ChangeScreen(new SelectScreen());

        /// <summary>
        ///     Loads up local scores and watches the replay.
        /// </summary>
        public void WatchReplay()
        {
            var scores = LocalScoreCache.FetchMapScores(MapManager.Selected.Value.Md5Checksum);

            // If the replay is from a local score, then read the replay here.
            // NOTE: If loading from gameplay/replay file, the replay to use is already established.
            if (Type == ResultsScreenType.FromLocalScore)
            {
                try
                {
                    Replay = new Replay(LocalReplayPath);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, LogType.Runtime);
                    ScreenManager.ChangeScreen(new SelectScreen());
                    NotificationManager.Show(NotificationLevel.Error, "Error reading replay file.");
                    return;
                }
            }

            ScreenManager.ChangeScreen(new GameplayScreen(Qua, MapManager.Selected.Value.Md5Checksum, scores, Replay));
        }

        /// <summary>
        ///     Goes to the gameplay screen toplay the map.
        /// </summary>
        public void RetryMap()
        {
            var scores = LocalScoreCache.FetchMapScores(MapManager.Selected.Value.Md5Checksum);
            ScreenManager.ChangeScreen(new GameplayScreen(Qua, MapManager.Selected.Value.Md5Checksum, scores));
        }

        /// <summary>
        ///     Changes discord rich presence to show results.
        /// </summary>
        private void ChangeDiscordPresence()
        {
            DiscordManager.Client.CurrentPresence.Timestamps = null;

            // Don't change if we're loading in from a replay file.
            if (Type == ResultsScreenType.FromReplayFile || GameplayScreen.InReplayMode)
            {
                DiscordManager.Client.CurrentPresence.Details = "Idle";
                DiscordManager.Client.CurrentPresence.State = "In the menus";
                DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);
                return;
            }

            var state = GameplayScreen.Failed ? "Fail" : "Pass";
            var score = $"{ScoreProcessor.Score / 1000}k";
            var acc = $"{StringHelper.AccuracyToString(ScoreProcessor.Accuracy)}";
            var grade = GameplayScreen.Failed ? "F" : GradeHelper.GetGradeFromAccuracy(ScoreProcessor.Accuracy).ToString();
            var combo = $"{ScoreProcessor.MaxCombo}x";

            DiscordManager.Client.CurrentPresence.State = $"{state}: {grade} {score} {acc} {combo}";
            DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);
        }

    }
}
