using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Gameplay;
using Quaver.API.Maps;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UI;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Gameplay.UI.Components;
using Quaver.States.Gameplay.UI.Components.Judgements;
using Quaver.States.Gameplay.UI.Components.Judgements.Counter;
using Quaver.States.Gameplay.UI.Components.Pause;
using Quaver.States.Gameplay.UI.Components.Scoreboard;
using Quaver.States.Results;

namespace Quaver.States.Gameplay.UI
{
    internal class GameplayInterface : IGameStateComponent
    {
        /// <summary>
        ///     Reference to the gameplay screen itself.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     Contains general purpose stuff for this screen such as the following:
        ///         - Score/Accuracy Display
        ///         - Leaderboard display
        ///         - Song progress time display
        /// </summary>
        private Container Container { get; }

        /// <summary>
        ///     The progress bar for the song time.
        /// </summary>
        private SongTimeProgressBar SongTimeProgressBar { get; set;  }

        /// <summary>
        ///     The display for score.
        /// </summary>
        private NumberDisplay ScoreDisplay { get; set; }

        /// <summary>
        ///     The display for accuracy.
        /// </summary>
        private NumberDisplay AccuracyDisplay { get; set; }

        /// <summary>
        ///     Displays the judgements and KPS if specified.
        /// </summary>
        private JudgementCounter JudgementCounter { get; set; }

        /// <summary>
        ///     The sprite used solely to fade the screen with transitions.
        /// </summary>
        internal Sprite ScreenTransitioner { get; set; }

        /// <summary>
        ///     The keys per second display.
        /// </summary>
        internal KeysPerSecond KpsDisplay { get; set; }

        /// <summary>
        ///     Sprite that displays the grade next to the accuracy display
        /// </summary>
        internal GradeDisplay GradeDisplay { get; set;  }

        /// <summary>
        ///     Overlay that's shown when paused.
        /// </summary>
        internal PauseOverlay PauseOverlay { get; set; }

        /// <summary>
        ///     Song information displayed at the beginning of the map.
        /// </summary>
        internal SongInformation SongInfo { get; set; }

        /// <summary>
        ///     If the volume has already been set to fade out.
        /// </summary>
        private bool VolumeFadedOut { get; set; }

        /// <summary>
        ///     The time counter where the game should start fading out.
        /// </summary>
        private double GameShouldFadeOutTime { get; set; }

        /// <summary>
        ///     The amount of time for the overlay to fade in/out
        /// </summary>
        internal int PauseFadeTimeScale { get; } = 120;

        /// <summary>
        ///     The scoreboard, lol.
        /// </summary>
        internal Scoreboard Scoreboard { get; set; }

        /// <summary>
        ///     Displayed when a user is eligible to skip.
        /// </summary>
        internal SkipDisplay SkipDisplay { get; private set; }

        /// <summary>
        ///     If we've already initiated the load of the results screen.
        /// </summary>
        private bool ResultsScreenLoadInitiated { get; set; }

        /// <summary>
        ///     If we're clear to exit the screen and the results screen has loaded.
        /// </summary>
        private bool ClearToExitScreen { get; set; }

        /// <summary>
        ///     The results screen in the future that we'll be going to after game completion.
        /// </summary>
        private ResultsScreen FutureResultScreen { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        internal GameplayInterface(GameplayScreen screen)
        {
            Screen = screen;
            Container = new Container();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            // Initialize the progress bar if the user has it set in config.
            if (ConfigManager.DisplaySongTimeProgress.Value)
                SongTimeProgressBar = new SongTimeProgressBar(Screen.Map.Length, 0, new Vector2(GameBase.WindowRectangle.Width, 6),
                                                            Container, Alignment.BotLeft);

            // Create score display
            ScoreDisplay = new NumberDisplay(NumberDisplayType.Score, StringHelper.ScoreToString(0), new Vector2(1.4f, 1.4f))
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                PosX = GameBase.Skin.Keys[Screen.Map.Mode].ScoreDisplayPosX,
                PosY = GameBase.Skin.Keys[Screen.Map.Mode].ScoreDisplayPosY
            };

            // Put the display in the top right corner.

            // Create acc display
            AccuracyDisplay = new NumberDisplay(NumberDisplayType.Accuracy, StringHelper.AccuracyToString(0), new Vector2(1.4f, 1.4f))
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
            };

            // Set the position of the accuracy display.
            AccuracyDisplay.PosX = -AccuracyDisplay.TotalWidth + GameBase.Skin.Keys[Screen.Map.Mode].AccuracyDisplayPosX;
            AccuracyDisplay.PosY = GameBase.Skin.Keys[Screen.Map.Mode].AccuracyDisplayPosY;

            // Create judgement status display
            JudgementCounter = new JudgementCounter(Screen) { Parent = Container };

            // Create KPS display
            KpsDisplay = new KeysPerSecond(NumberDisplayType.Score, "0", new Vector2(1.75f, 1.75f))
            {
                Parent = Container,
                Alignment = Alignment.TopRight
            };

            // Set the position of the KPS display
            KpsDisplay.PosX = -KpsDisplay.TotalWidth + GameBase.Skin.Keys[Screen.Map.Mode].KpsDisplayPosX;
            KpsDisplay.PosY = AccuracyDisplay.PosY + AccuracyDisplay.Digits[0].SizeY + GameBase.Skin.Keys[Screen.Map.Mode].KpsDisplayPosY;

            GradeDisplay = new GradeDisplay(Screen.Ruleset.ScoreProcessor)
            {
                Parent = Container,
                Size = new UDim2D(AccuracyDisplay.Digits[0].SizeX, AccuracyDisplay.Digits[0].SizeY),
                Alignment = Alignment.TopRight,
                Position = new UDim2D(GetGradeDisplayPosX(), AccuracyDisplay.PosY)
            };

            // Song Information Display
            SongInfo = new SongInformation(Screen)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = -200
            };

            // Create scoreboard
            CreateScoreboard();

            SkipDisplay = new SkipDisplay(Screen, GameBase.Skin.Skip)
            {
                Parent = Container
            };

            // Initialize the failure trannsitioner.
            ScreenTransitioner = new Sprite()
            {
                Parent = Container,
                ScaleX = 1,
                ScaleY = 1,
                Tint = Color.Black,
                Alpha = 1
            };

            PauseOverlay = new PauseOverlay(Screen) {Parent = Container};
        }

        /// <summary>
        ///     Destroy
        /// </summary>
        public void UnloadContent() => Container.Destroy();

        /// <summary>
        ///
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            // Hide navbar in gameplay
            if (!Screen.IsPaused && !Screen.Failed)
            {
                GameBase.Navbar.PerformHideAnimation(dt);
                BackgroundManager.Readjust();
            }

            // Fade the cursor depending on if the user is paused or not.
            if (Screen.IsPaused && !Screen.IsResumeInProgress)
                GameBase.Cursor.FadeIn(dt, 240);
            else
                GameBase.Cursor.FadeOut(dt, 240);

            UpdateSongProgressDisplay();
            UpdateScoreAndAccuracyDisplays();

            // Change grade display posX
            GradeDisplay.PosX = GetGradeDisplayPosX();

            HandlePlayCompletion(dt);
            HandlePause(dt);
            FadeInScreen(dt);

            Container.Update(dt);
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw() => Container.Draw();

        /// <summary>
        ///     Updates the number displays
        ///     Score and Accuracy
        /// </summary>
        private void UpdateScoreAndAccuracyDisplays()
        {
            // Update score and accuracy displays
            ScoreDisplay.Value = StringHelper.ScoreToString(Screen.Ruleset.ScoreProcessor.Score);

            // Grab the old accuracy
            var oldAcc = AccuracyDisplay.Value;

            // Update the new accuracy.
            AccuracyDisplay.Value = StringHelper.AccuracyToString(Screen.Ruleset.ScoreProcessor.Accuracy);

            // If the old accuracy's length isn't the same, then we need to reposition the sprite
            // Example: 100.00% to 99.99% needs repositioning.
            if (oldAcc.Length != AccuracyDisplay.Value.Length)
                AccuracyDisplay.PosX = -AccuracyDisplay.TotalWidth + GameBase.Skin.Keys[Screen.Map.Mode].AccuracyDisplayPosX;
        }

        /// <summary>
        ///     Updates the song progress display.
        /// </summary>
        private void UpdateSongProgressDisplay()
        {
            // Update the current value of the song time progress bar if it is actually initialized
            // and the user wants to actually display it.
            if (ConfigManager.DisplaySongTimeProgress.Value && SongTimeProgressBar != null)
                SongTimeProgressBar.CurrentValue = (float) Screen.Timing.CurrentTime;
        }

        /// <summary>
        ///     Handles the fadeout with failure/play completion.
        /// </summary>
        private void HandlePlayCompletion(double dt)
        {
            if (!Screen.Failed && !Screen.IsPlayComplete)
                return;

            // Wait a bit before actually fading out the game.
            GameShouldFadeOutTime += dt;
            if (GameShouldFadeOutTime <= 800)
                return;

            Screen.Ruleset.Playfield.HandleFailure(dt);

            // Fade Out audio
            if (GameBase.AudioEngine.IsPlaying && !VolumeFadedOut)
            {
                VolumeFadedOut = true;
                AudioEngine.Fade(0, 1800);
            }

            // Load the results screen asynchronously, so that we don't run through any freezes.
            if (!ResultsScreenLoadInitiated)
            {
                GameStateManager.LoadAsync(() =>
                {
                    FutureResultScreen = new ResultsScreen(Screen);
                    return FutureResultScreen;
                },
                () => ClearToExitScreen = true);

                ResultsScreenLoadInitiated = true;
            }

            if (!ClearToExitScreen)
                return;

            ScreenTransitioner.FadeIn(dt, 120);

            // Increase time after the user failed.
            Screen.TimeSincePlayEnded += dt;

            // Change to the results screen
            if (Screen.TimeSincePlayEnded >= 1200)
                GameBase.GameStateManager.ChangeState(FutureResultScreen);
        }

        /// <summary>
        ///     Handle pausing & unpausing UI.
        /// </summary>
        /// <param name="dt"></param>
        private void HandlePause(double dt)
        {
            if (!Screen.IsPaused)
                return;

            if (Screen.IsResumeInProgress)
                ScreenTransitioner.Fade(dt, 0, PauseFadeTimeScale * 2f);
            else
                ScreenTransitioner.Fade(dt, 0.90f, PauseFadeTimeScale);
        }

        /// <summary>
        ///     Fades in the screen ready for gameplay.
        /// </summary>
        /// <param name="dt"></param>
        private void FadeInScreen(double dt)
        {
            if (!Screen.IsPaused && !Screen.IsResumeInProgress && !Screen.IsRestartingPlay && !Screen.Failed && !Screen.IsPlayComplete)
                ScreenTransitioner.FadeOut(dt, 720);
        }

        /// <summary>
        ///     F
        /// </summary>
        /// <returns></returns>
        private float GetGradeDisplayPosX() => AccuracyDisplay.PosX - 8;

        /// <summary>
        ///     Creates all of the scoreboard users.
        /// </summary>
        private void CreateScoreboard()
        {
            // Use the replay's name for the scoreboard if we're watching one.
            var scoreboardName = Screen.InReplayMode ? Screen.LoadedReplay.PlayerName : ConfigManager.Username.Value;
            var users = new List<ScoreboardUser>
            {
                // Add ourself to the list of scoreboard users first.
                new ScoreboardUser(Screen, ScoreboardUserType.Self, scoreboardName, null, UserInterface.YouAvatar)
                {
                    Parent = Container,
                    Alignment = Alignment.MidLeft
                }
            };

            // Add local scores.
            for (var i = 0; i < Screen.LocalScores.Count && i < 5; i++)
            {
                // Decompress score
                var scoreJudgements = new List<Judgement>();

                // Decompress the local score and add all the judgements to the list
                foreach (var c in GzipHelper.Decompress(Screen.LocalScores[i].JudgementBreakdown))
                    scoreJudgements.Add((Judgement)int.Parse(c.ToString()));

                users.Add(new ScoreboardUser(Screen, ScoreboardUserType.Other, $"{Screen.LocalScores[i].Name} #{i + 1}", scoreJudgements, UserInterface.UnknownAvatar)
                {
                    Parent = Container,
                    Alignment = Alignment.MidLeft
                });
            }

            // Create bots on the scoreboard.
            if (ConfigManager.BotsEnabled.Value)
            {
                // Generate bots users on the scoreboard if need be.
                for (var i = 0; i < ConfigManager.BotCount.Value && users.Count - 1 < ConfigManager.BotCount.Value; i++)
                {
                    // Create new bot.
                    var bot = new Bot(Screen.Map, BotLevel.Decent);

                    // Keep selecting usernames if we have duplicate bot names.
                    while (users.Any(x => x.Username.Text.Contains(bot.Name)))
                        bot.Name = Bot.GenerateRandomName();

                    users.Add(new ScoreboardUser(Screen, ScoreboardUserType.Other, bot.Name, bot.Judgements, UserInterface.UnknownAvatar)
                    {
                        Parent = Container,
                        Alignment = Alignment.MidLeft
                    });
                }
            }

            Scoreboard = new Scoreboard(users) {Parent = Container};
        }

        /// <summary>
        ///     Updates the scoreboard for all the current users.
        /// </summary>
        internal void UpdateScoreboardUsers() => Scoreboard.CalculateScores();
    }
}