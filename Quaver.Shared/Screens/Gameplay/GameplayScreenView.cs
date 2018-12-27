/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Gameplay.UI;
using Quaver.Shared.Screens.Gameplay.UI.Counter;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Quaver.Shared.Screens.Result;
using Quaver.Shared.Skinning;
using Steamworks;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Gameplay
{
    public class GameplayScreenView : ScreenView
    {
        /// <summary>
        ///     Reference to the gameplay screen.
        /// </summary>
        public new GameplayScreen Screen { get; }

        /// <summary>
        ///     The map's background.
        /// </summary>
        public BackgroundImage Background { get; private set; }

        /// <summary>
        ///     The progress bar that displays the current song time.
        /// </summary>
        private SongTimeProgressBar ProgressBar { get; set; }

        /// <summary>
        ///     The display for the user's score.
        /// </summary>
        private NumberDisplay ScoreDisplay { get; set; }

        /// <summary>
        ///     The display for the user's accuracy
        /// </summary>
        private NumberDisplay AccuracyDisplay { get; set; }

        /// <summary>
        ///     The keys per second display.
        /// </summary>
        public KeysPerSecond KpsDisplay { get; set; }

        /// <summary>
        ///     Displays the current judgement counts.
        /// </summary>
        private JudgementCounter JudgementCounter { get; set; }

        /// <summary>
        ///     Displays the user's current grade.
        /// </summary>
        private GradeDisplay GradeDisplay { get; set; }

        /// <summary>
        ///     The x position of the grade display
        /// </summary>
        private float GradeDisplayX => AccuracyDisplay.X - 8;

        /// <summary>
        ///     The scoreboard
        /// </summary>
        public Scoreboard Scoreboard { get; set; }

        /// <summary>
        ///     The display to skip the map.
        /// </summary>
        public SkipDisplay SkipDisplay { get; set; }

        /// <summary>
        ///     The sprite used solely to fade the screen with transitions.
        /// </summary>
        public Sprite Transitioner { get; set; }

        /// <summary>
        ///     The pause overlay for the screen.
        /// </summary>
        public PauseScreen PauseScreen { get; set; }

        /// <summary>
        ///     Determines if the transitioner is currently fading on play restart.
        /// </summary>
        public bool FadingOnRestartKeyPress { get; set; }

        /// <summary>
        ///     Determines if the transitioner is currently fading on play restart release.
        ///     When the user presses the release key, but not fully. They let it go.
        /// </summary>
        public bool FadingOnRestartKeyRelease { get; set; }

        /// <summary>
        ///     Determines if the transitioner is currently fading on play completion.
        /// </summary>
        public bool FadingOnPlayCompletion { get; set; }

        /// <summary>
        ///     Determines if when the play has failed, the screen was turned to red.
        /// </summary>
        public bool ScreenChangedToRedOnFailure { get; set; }

        /// <summary>
        ///     When true, the results screen is currently loading asynchronously.
        /// </summary>
        private bool ResultsScreenLoadInitiated { get; set; }

        /// <summary>
        ///     The results screen to be loaded in the future on play completion.
        /// </summary>
        private QuaverScreen FutureResultsScreen { get; set; }

        /// <summary>
        ///     When the results screen has successfully loaded, we'll be considered clear
        ///     to exit and fade out the screen.
        /// </summary>
        private bool ClearToExitScreen { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public GameplayScreenView(Screen screen) : base(screen)
        {
            Screen = (GameplayScreen)screen;

            CreateBackground();
            CreateProgressBar();
            CreateScoreDisplay();
            CreateAccuracyDisplay();

            // Create judgement status display
            JudgementCounter = new JudgementCounter(Screen) { Parent = Container };

            CreateKeysPerSecondDisplay();
            CreateGradeDisplay();
            CreateScoreboard();

            SkipDisplay = new SkipDisplay(Screen, SkinManager.Skin.Skip) { Parent = Container };

            // Create screen transitioner to perform any animations.
            Transitioner = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Tint = Color.Black,
                Alpha = 1,
                Animations =
                {
                    // Fade in from black.
                    new Animation(AnimationProperty.Alpha, Easing.Linear, 1, 0, 1500)
                }
            };

            // Create pause screen last.
            PauseScreen = new PauseScreen(Screen) { Parent = Container };

            // Notify the user if their local offset is actually set for this map.
            if (MapManager.Selected.Value.LocalOffset != 0)
                NotificationManager.Show(NotificationLevel.Info, $"The local audio offset for this map is: {MapManager.Selected.Value.LocalOffset}ms");
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            CheckIfNewScoreboardUsers();
            UpdateScoreAndAccuracyDisplays();
            GradeDisplay.X = GradeDisplayX;
            HandlePlayCompletion(gameTime);
            Screen.Ruleset?.Update(gameTime);
            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);

            Background.Draw(gameTime);
            Screen.Ruleset?.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Screen.Ruleset?.Destroy();
            Container?.Destroy();
        }

        /// <summary>
        ///     Creates the background sprite for the screen.
        /// </summary>
        private void CreateBackground()
        {
            var background = ConfigManager.BlurBackgroundInGameplay.Value ? BackgroundHelper.BlurredTexture : BackgroundHelper.RawTexture;

            // We don't set a parent here because we have to manually call draw on the background, as the
            // ScreenView's container is drawn after the ruleset.
            Background = new BackgroundImage(background, 100 - ConfigManager.BackgroundBrightness.Value, false);
        }

        /// <summary>
        ///     Creates the progress bar if the user defined it in config.
        /// </summary>
        private void CreateProgressBar()
        {
            if (!ConfigManager.DisplaySongTimeProgress.Value)
                return;

            ProgressBar = new SongTimeProgressBar(Screen, new Vector2(WindowManager.Width, 4), 0, Screen.Map.Length, 0,
                Colors.MainAccentInactive, Colors.MainAccent)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };
        }

        /// <summary>
        ///     Creates the score display sprite.
        /// </summary>
        private void CreateScoreDisplay() => ScoreDisplay = new NumberDisplay(NumberDisplayType.Score, StringHelper.ScoreToString(0),
                                                                                new Vector2(0.45f, 0.45f))
        {
            Parent = Container,
            Alignment = Alignment.TopLeft,
            X = SkinManager.Skin.Keys[Screen.Map.Mode].ScoreDisplayPosX,
            Y = SkinManager.Skin.Keys[Screen.Map.Mode].ScoreDisplayPosY
        };

        /// <summary>
        ///     Creates the accuracy display sprite.
        /// </summary>
        private void CreateAccuracyDisplay()
        {
            AccuracyDisplay = new NumberDisplay(NumberDisplayType.Accuracy, StringHelper.AccuracyToString(0), new Vector2(0.45f, 0.45f))
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
            };

            // Set the position of the accuracy display.
            AccuracyDisplay.X = -AccuracyDisplay.TotalWidth + SkinManager.Skin.Keys[Screen.Map.Mode].AccuracyDisplayPosX;
            AccuracyDisplay.Y = SkinManager.Skin.Keys[Screen.Map.Mode].AccuracyDisplayPosY;
        }

        /// <summary>
        ///     Updates the values and positions of the score and accuracy displays.
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
                AccuracyDisplay.X = -AccuracyDisplay.TotalWidth + SkinManager.Skin.Keys[Screen.Map.Mode].AccuracyDisplayPosX;
        }

        /// <summary>
        ///     Creates the display for KPS
        /// </summary>
        private void CreateKeysPerSecondDisplay()
        {
            // Create KPS display
            KpsDisplay = new KeysPerSecond(NumberDisplayType.Score, "0", new Vector2(0.45f, 0.45f))
            {
                Parent = Container,
                Alignment = Alignment.TopRight
            };

            // Set the position of the KPS display
            KpsDisplay.X = -KpsDisplay.TotalWidth + SkinManager.Skin.Keys[Screen.Map.Mode].KpsDisplayPosX;
            KpsDisplay.Y = AccuracyDisplay.Y + AccuracyDisplay.Digits[0].Height + SkinManager.Skin.Keys[Screen.Map.Mode].KpsDisplayPosY;
        }

        /// <summary>
        ///     Creates the GradeDisplay sprite
        /// </summary>
        private void CreateGradeDisplay() => GradeDisplay = new GradeDisplay(Screen.Ruleset.ScoreProcessor)
        {
            Parent = Container,
            Size = new ScalableVector2(AccuracyDisplay.Digits[0].Width, AccuracyDisplay.Digits[0].Height),
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(GradeDisplayX, AccuracyDisplay.Y)
        };

        /// <summary>
        ///     Creates the scoreboard for the game.
        /// </summary>
        private void CreateScoreboard()
        {
            // Use the replay's name for the scoreboard if we're watching one.
            var scoreboardName = Screen.InReplayMode ? Screen.LoadedReplay.PlayerName : ConfigManager.Username.Value;

            var selfAatar = ConfigManager.Username.Value == scoreboardName ? SteamManager.UserAvatars[SteamUser.GetSteamID().m_SteamID]
                : UserInterface.UnknownAvatar;

            var users = new List<ScoreboardUser>
            {
                // Add ourself to the list of scoreboard users first.
                new ScoreboardUser(Screen, ScoreboardUserType.Self, scoreboardName, null, selfAatar,
                    ModManager.Mods)
                {
                    Parent = Container,
                    Alignment = Alignment.MidLeft
                }
            };

            Scoreboard = new Scoreboard(users) { Parent = Container };
        }

        /// <summary>
        ///     Updates the scoreboard for all the current users.
        /// </summary>
        public void UpdateScoreboardUsers() => Scoreboard?.CalculateScores();

        /// <summary>
        ///     Checks if there are new scoreboard users.
        /// </summary>
        private void CheckIfNewScoreboardUsers()
        {
            var mapScores = MapManager.Selected.Value.Scores.Value;

            if (mapScores == null || mapScores.Count <= 0 || Scoreboard.Users.Count != 1)
                return;

            for (var i = 0; i < 4 && i < mapScores.Count; i++)
            {
                ScoreboardUser user;

                // For online scores we want to just give them their score in the processor,
                // since we don't have access to their judgement breakdown.
                if (mapScores[i].IsOnline)
                {
                    user = new ScoreboardUser(Screen, ScoreboardUserType.Other, $"{mapScores[i].Name}",
                        new List<Judgement>(), UserInterface.UnknownAvatar, mapScores[i].Mods, mapScores[i])
                    {
                        Parent = Container,
                        Alignment = Alignment.MidLeft
                    };

                    user.Scoreboard = Scoreboard;

                    var processor = user.Processor as ScoreProcessorKeys;
                    processor.Accuracy = (float) mapScores[i].Accuracy;
                    processor.MaxCombo = mapScores[i].MaxCombo;
                    processor.Score = mapScores[i].TotalScore;

                    user.Score.Text = $"{user.RatingProcessor.CalculateRating(processor.Accuracy):0.00} / {StringHelper.AccuracyToString(processor.Accuracy)}";
                    user.Combo.Text = $"{processor.MaxCombo}x";
                }
                // Allow the user to play against their own local scores.
                else
                {
                    // Decompress score
                    var breakdownHits = GzipHelper.Decompress(mapScores[i].JudgementBreakdown);

                    var judgements = new List<Judgement>();

                    // Get all of the hit stats for the score.
                    foreach (var hit in breakdownHits)
                        judgements.Add((Judgement)int.Parse(hit.ToString()));

                    user = new ScoreboardUser(Screen, ScoreboardUserType.Other, $"{mapScores[i].Name}",
                        judgements, UserInterface.UnknownAvatar, mapScores[i].Mods, mapScores[i])
                    {
                        Parent = Container,
                        Alignment = Alignment.MidLeft
                    };

                    user.Scoreboard = Scoreboard;

                    // Make sure the user's score is updated with the current user.
                    for (var j = 0; j < Screen.Ruleset.ScoreProcessor.TotalJudgementCount && i < judgements.Count; j++)
                    {
                        var processor = user.Processor as ScoreProcessorKeys;
                        processor?.CalculateScore(judgements[i]);
                    }

                }

                Scoreboard.Users.Add(user);
            }

            Scoreboard.SetTargetYPositions();

            // Re-change the transitioner and pause screen's parent so that they appear on top of the scoreboard
            // again.
            Transitioner.Parent = Container;
            PauseScreen.Parent = Container;
        }

        /// <summary>
        ///     Starts the fade out process for the game on play completion.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandlePlayCompletion(GameTime gameTime)
        {
            if (!Screen.Failed && !Screen.IsPlayComplete)
                return;

            Screen.TimeSincePlayEnded += gameTime.ElapsedGameTime.TotalMilliseconds;

            // If the play was a failure, we want to immediately show
            // a red screen.
            if (Screen.Failed && !ScreenChangedToRedOnFailure)
            {
                Transitioner.Tint = Color.Red;
                Transitioner.Alpha = 0.65f;

                ScreenChangedToRedOnFailure = true;
            }

            // Load the results screen asynchronously, so that we don't run through any freezes.
            if (!ResultsScreenLoadInitiated)
            {
                Screen.Exit(() => new ResultScreen(Screen), 500);
                ResultsScreenLoadInitiated = true;
            }

            // Don't fade unless we're fully clear to do so.
            if (Screen.TimeSincePlayEnded <= 1200 || !ClearToExitScreen)
                return;

            // If the play was a failure, immediately start fading to black.
            if (Screen.Failed)
                Transitioner.FadeToColor(Color.Black, gameTime.ElapsedGameTime.TotalMilliseconds, 150);

            // Start fading out the screen.
            if (!FadingOnPlayCompletion)
            {
                Transitioner.Animations.Clear();

                // Get the initial alpha of the sceen transitioner, because it can be different based
                // on if the user failed or not, and use this in the Animation
                var initialAlpha = Screen.Failed ? 0.65f : 0;

                Transitioner.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, initialAlpha, 1, 1000));
                FadingOnPlayCompletion = true;
            }

            if (Screen.TimeSincePlayEnded >= 3000)
            {
                // Change background dim before switching screens.
                BackgroundManager.Background.Dim = 0;
            }
        }

        /// <summary>
        ///     When a background is loaded in the gameplay screen (because multi-threading....),
        ///     we'll want to fade it in to the user's set dim.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            if (e.Map != MapManager.Selected.Value)
                return;

            FadeBackgroundToDim();
        }

        private void FadeBackgroundToDim()
        {
            BackgroundManager.Background.BrightnessSprite.Animations.Clear();

            var t = new Animation(AnimationProperty.Alpha, Easing.Linear, BackgroundManager.Background.BrightnessSprite.Alpha,
                (100 - ConfigManager.BackgroundBrightness.Value) / 100f, 300);

            BackgroundManager.Background.BrightnessSprite.Animations.Add(t);

            // Blur background strength
            // BackgroundManager.Background.Strength = ConfigManager.BackgroundBlur.Value;
        }
    }
}
