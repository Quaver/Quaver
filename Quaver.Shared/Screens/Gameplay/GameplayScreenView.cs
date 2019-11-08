/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Gameplay.UI;
using Quaver.Shared.Screens.Gameplay.UI.Counter;
using Quaver.Shared.Screens.Gameplay.UI.Multiplayer;
using Quaver.Shared.Screens.Gameplay.UI.Offset;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.Result;
using Quaver.Shared.Screens.Select;
using Quaver.Shared.Skinning;
using Steamworks;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Logging;
using Wobble.Screens;
using Wobble.Window;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Quaver.Shared.Screens.Gameplay
{
    public class GameplayScreenView : ScreenView
    {
        /// <summary>
        ///     Reference to the gameplay screen.
        /// </summary>
        public new GameplayScreen Screen { get; }

        /// <summary>
        ///     Handles calculating rating
        /// </summary>
        internal RatingProcessorKeys RatingProcessor { get; }

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
        ///     The display for the user's rating.
        /// </summary>
        private NumberDisplay RatingDisplay { get; set; }

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
        ///     The scoreboard on the left side of the screern
        ///     (normal OR red team)
        ///     The scoreboard
        /// </summary>
        public Scoreboard ScoreboardLeft { get; set; }

        /// <summary>
        ///     The scoreboard on the right side of the screen
        /// </summary>
        public Scoreboard ScoreboardRight { get; set; }

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
        /// </summary>
        private ComboAlert ComboAlert { get; set; }

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
        ///     When the results screen has successfully loaded, we'll be considered clear
        ///     to exit and fade out the screen.
        /// </summary>
        private bool ClearToExitScreen { get; set; }

        /// <summary>
        /// </summary>
        private OffsetCalibratorTip Tip { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerEndGameWaitTime MultiplayerEndTime { get; set; }

        /// <summary>
        ///     If true, the game will stop waiting for new scoreboard users
        /// </summary>
        private bool StopCheckingForScoreboardUsers { get; set; }

        /// <summary>
        /// </summary>
        private BattleRoyaleBackgroundAlerter BattleRoyaleBackgroundAlerter { get; }

        /// <summary>
        /// </summary>
        public ScoreboardUser SelfScoreboard { get; private set; }

        /// <summary>
        /// </summary>
        private SpectatorDialog SpectatorDialog { get; set; }

        /// <summary>
        /// </summary>
        private SpectatorCount SpectatorCount { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public GameplayScreenView(Screen screen) : base(screen)
        {
            Screen = (GameplayScreen)screen;
            RatingProcessor = new RatingProcessorKeys(MapManager.Selected.Value.DifficultyFromMods(Screen.Ruleset.ScoreProcessor.Mods));

            CreateBackground();

            if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Battle_Royale
                                                  && ConfigManager.EnableBattleRoyaleBackgroundFlashing.Value)
            {
                BattleRoyaleBackgroundAlerter = new BattleRoyaleBackgroundAlerter(this);
            }

            if (!Screen.IsPlayTesting && !Screen.IsCalibratingOffset)
                CreateScoreboards();

            CreateProgressBar();
            CreateScoreDisplay();
            CreateRatingDisplay();
            CreateAccuracyDisplay();

            if (ConfigManager.DisplayComboAlerts.Value)
                ComboAlert = new ComboAlert(Screen.Ruleset.ScoreProcessor) { Parent = Container };

            // Create judgement status display
            if (ConfigManager.DisplayJudgementCounter.Value)
            {
                if (OnlineManager.CurrentGame == null || OnlineManager.CurrentGame.Ruleset != MultiplayerGameRuleset.Team)
                    JudgementCounter = new JudgementCounter(Screen) { Parent = Container };
            }

            CreateKeysPerSecondDisplay();
            CreateGradeDisplay();

            SkipDisplay = new SkipDisplay(Screen, SkinManager.Skin.Skip) { Parent = Container };

            if (Screen.IsMultiplayerGame)
            {
                MultiplayerEndTime = new MultiplayerEndGameWaitTime
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter
                };
            }

            if (Screen.SpectatorClient != null)
            {
                SpectatorDialog = new SpectatorDialog(Screen.SpectatorClient)
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter,
                    Alpha = 0
                };
            }

            SpectatorCount = new SpectatorCount
            {
                Parent = Container,
                Y = 120,
                Alignment = Alignment.TopRight,
                X = -10
            };

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
            if (Screen.SpectatorClient == null)
                PauseScreen = new PauseScreen(Screen) { Parent = Container };

            // Notify the user if their local offset is actually set for this map.
            if (MapManager.Selected.Value.LocalOffset != 0)
                NotificationManager.Show(NotificationLevel.Info, $"The local audio offset for this map is: {MapManager.Selected.Value.LocalOffset} ms");

            if (Screen.IsCalibratingOffset)
            {
                Tip = new OffsetCalibratorTip
                {
                    Parent = Container,
                    Alignment = Alignment.MidCenter
                };
            }

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameEnded += OnGameEnded;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleWaitingForPlayersDialog();
            CheckIfNewScoreboardUsers();
            HandlePlayCompletion(gameTime);
            BattleRoyaleBackgroundAlerter?.Update(gameTime);
            Screen.Ruleset?.Update(gameTime);
            Container?.Update(gameTime);

            // Update the position and size of the grade display.
            GradeDisplay.X = AccuracyDisplay.X - AccuracyDisplay.Width - 8;
            GradeDisplay.Height = AccuracyDisplay.Height;
            GradeDisplay.UpdateWidth();

            if (SpectatorDialog != null)
            {
                SpectatorDialog.Alpha = MathHelper.Lerp(SpectatorDialog.Alpha, Screen.IsPaused ? 1 : 0,
                    (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 100, 1));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);

            Background.Draw(gameTime);
            BattleRoyaleBackgroundAlerter?.Draw(gameTime);
            Screen.Ruleset?.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameEnded -= OnGameEnded;

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

            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

            ProgressBar = new SongTimeProgressBar(Screen, new Vector2(WindowManager.Width, 4), 0, Screen.Map.Length / ModHelper.GetRateFromMods(ModManager.Mods), 0,
                skin.SongTimeProgressInactiveColor, skin.SongTimeProgressActiveColor)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };
        }

        /// <summary>
        ///     Creates the score display sprite.
        /// </summary>
        private void CreateScoreDisplay()
        {
            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

            ScoreDisplay = new NumberDisplay(NumberDisplayType.Score, StringHelper.ScoreToString(0),
                new Vector2(skin.ScoreDisplayScale / 100f, skin.ScoreDisplayScale / 100f))
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                X = SkinManager.Skin.Keys[Screen.Map.Mode].ScoreDisplayPosX,
                Y = SkinManager.Skin.Keys[Screen.Map.Mode].ScoreDisplayPosY
            };
        }

        /// <summary>
        ///     Creates the rating display sprite.
        /// </summary>
        private void CreateRatingDisplay()
        {
            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

            RatingDisplay = new NumberDisplay(NumberDisplayType.Rating, StringHelper.RatingToString(0),
                new Vector2(skin.RatingDisplayScale / 100f, skin.RatingDisplayScale / 100f))
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                X = SkinManager.Skin.Keys[Screen.Map.Mode].RatingDisplayPosX,
                Y = 40 + SkinManager.Skin.Keys[Screen.Map.Mode].RatingDisplayPosY
            };
        }

        /// <summary>
        ///     Creates the accuracy display sprite.
        /// </summary>
        private void CreateAccuracyDisplay()
        {
            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

            AccuracyDisplay = new NumberDisplay(NumberDisplayType.Accuracy, StringHelper.AccuracyToString(0),
                new Vector2(skin.AccuracyDisplayScale / 100f, skin.AccuracyDisplayScale / 100f))
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                X = SkinManager.Skin.Keys[Screen.Map.Mode].AccuracyDisplayPosX,
                Y = SkinManager.Skin.Keys[Screen.Map.Mode].AccuracyDisplayPosY
            };
        }

        /// <summary>
        ///     Updates the values and positions of the score and accuracy displays.
        /// </summary>
        public void UpdateScoreAndAccuracyDisplays()
        {
            // Update score and accuracy displays
            ScoreDisplay.UpdateValue(Screen.Ruleset.ScoreProcessor.Score);
            RatingDisplay.UpdateValue(RatingProcessor.CalculateRating(Screen.Ruleset.StandardizedReplayPlayer.ScoreProcessor.Accuracy));
            AccuracyDisplay.UpdateValue(Screen.Ruleset.ScoreProcessor.Accuracy);
        }

        /// <summary>
        ///     Creates the display for KPS
        /// </summary>
        private void CreateKeysPerSecondDisplay()
        {
            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

            // Create KPS display
            KpsDisplay = new KeysPerSecond(NumberDisplayType.Score, "0", new Vector2(skin.KpsDisplayScale / 100f, skin.KpsDisplayScale / 100f))
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                X = SkinManager.Skin.Keys[Screen.Map.Mode].KpsDisplayPosX,
                Y = 40 + SkinManager.Skin.Keys[Screen.Map.Mode].KpsDisplayPosY
            };
        }

        /// <summary>
        ///     Creates the GradeDisplay sprite
        /// </summary>
        private void CreateGradeDisplay() => GradeDisplay = new GradeDisplay(Screen.Ruleset.ScoreProcessor)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            X = AccuracyDisplay.X - AccuracyDisplay.Width - 8,
            Y = AccuracyDisplay.Y
        };

        /// <summary>
        ///     Creates the scoreboard for the game.
        /// </summary>
        private void CreateScoreboards()
        {
            // Use the replay's name for the scoreboard if we're watching one.
            var scoreboardName = Screen.InReplayMode ? Screen.LoadedReplay.PlayerName : ConfigManager.Username.Value;

            var selfAvatar = ConfigManager.Username.Value == scoreboardName ? SteamManager.UserAvatars[SteamUser.GetSteamID().m_SteamID]
                : UserInterface.UnknownAvatar;

            SelfScoreboard = new ScoreboardUser(Screen, ScoreboardUserType.Self, scoreboardName, null, selfAvatar,
                ModManager.Mods)
            {
                Parent = Container,
                Alignment = Alignment.MidLeft
            };

            var users = new List<ScoreboardUser> {SelfScoreboard};

            if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
            {
                // Blue Team
                ScoreboardRight = new Scoreboard(ScoreboardType.Teams,
                    OnlineManager.GetTeam(OnlineManager.Self.OnlineUser.Id) == MultiplayerTeam.Blue ? users : new List<ScoreboardUser>(), MultiplayerTeam.Blue)
                {
                    Parent = Container,
                    Alignment = Alignment.TopLeft,
                };
            }

            var scoreboardType = OnlineManager.CurrentGame != null &&
                                 OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team
                                ? ScoreboardType.Teams
                                : ScoreboardType.FreeForAll;

            // Red team/normal leaderboard
            ScoreboardLeft = new Scoreboard(scoreboardType,
                OnlineManager.CurrentGame == null || OnlineManager.GetTeam(OnlineManager.Self.OnlineUser.Id) == MultiplayerTeam.Red ?
                    users : new List<ScoreboardUser>()) { Parent = Container };

            ScoreboardLeft?.Users.ForEach(x => x.SetImage());
            ScoreboardRight?.Users.ForEach(x => x.SetImage());
        }

        /// <summary>
        ///     Updates the scoreboard for all the current users.
        /// </summary>
        public void UpdateScoreboardUsers()
        {
            ScoreboardLeft?.CalculateScores();
            ScoreboardRight?.CalculateScores();
        }

        /// <summary>
        ///     Checks if there are new scoreboard users.
        /// </summary>
        private void CheckIfNewScoreboardUsers()
        {
            if (Screen.IsPlayTesting || StopCheckingForScoreboardUsers)
                return;

            var mapScores = MapManager.Selected.Value.Scores.Value;

            if (mapScores == null || mapScores.Count <= 0 || (ScoreboardLeft.Users?.Count < 1 && ScoreboardRight != null && ScoreboardRight.Users.Count < 1))
                return;

            for (var i = 0; i < (OnlineManager.CurrentGame == null ? 4 : mapScores.Count) && i < mapScores.Count; i++)
            {
                ScoreboardUser user;

                // For online scores we want to just give them their score in the processor,
                // since we don't have access to their judgement breakdown.
                if (mapScores[i].IsOnline)
                {
                    user = new ScoreboardUser(Screen, ScoreboardUserType.Other, $"{mapScores[i].Name}",
                        new List<Judgement>(), UserInterface.UnknownAvatar, (ModIdentifier) mapScores[i].Mods, mapScores[i])
                    {
                        Parent = Container,
                        Alignment = Alignment.MidLeft
                    };

                    if (OnlineManager.CurrentGame != null &&
                        OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team && OnlineManager.GetTeam(user.LocalScore.PlayerId) == MultiplayerTeam.Blue)
                    {
                        user.Scoreboard = ScoreboardRight;
                        user.X = WindowManager.Width;
                    }
                    else
                    {
                        user.Scoreboard = ScoreboardLeft;
                    }

                    user.SetImage();

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
                        judgements, UserInterface.UnknownAvatar, (ModIdentifier) mapScores[i].Mods, mapScores[i])
                    {
                        Parent = Container,
                        Alignment = Alignment.MidLeft
                    };

                    if (OnlineManager.CurrentGame != null &&
                        OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team && OnlineManager.GetTeam(user.LocalScore.PlayerId) == MultiplayerTeam.Blue)
                    {
                        user.Scoreboard = ScoreboardRight;
                    }
                    else
                    {
                        user.Scoreboard = ScoreboardLeft;
                    }

                    user.SetImage();

                    // Make sure the user's score is updated with the current user.
                    for (var j = 0; j < Screen.Ruleset.ScoreProcessor.TotalJudgementCount && i < judgements.Count; j++)
                    {
                        var processor = user.Processor as ScoreProcessorKeys;
                        processor?.CalculateScore(judgements[i]);
                    }
                }

                if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team &&
                    OnlineManager.GetTeam(user.LocalScore.PlayerId) == MultiplayerTeam.Blue)
                {
                    ScoreboardRight.Users.Add(user);
                }
                else
                {
                    ScoreboardLeft.Users.Add(user);
                }
            }

            ScoreboardLeft.SetTargetYPositions();
            ScoreboardRight?.SetTargetYPositions();

            // Re-change the transitioner and pause screen's parent so that they appear on top of the scoreboard
            // again.
            if (ProgressBar != null)
                ProgressBar.Parent = Container;

            Transitioner.Parent = Container;

            if (PauseScreen != null)
                PauseScreen.Parent = Container;

            StopCheckingForScoreboardUsers = true;
            Screen.SetRichPresence();
        }

        /// <summary>
        ///     Starts the fade out process for the game on play completion.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandlePlayCompletion(GameTime gameTime)
        {
            if (!Screen.Failed && !Screen.IsPlayComplete || Screen.Exiting)
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

            if (!ResultsScreenLoadInitiated)
            {
                // Force all replay frames on failure
                if (OnlineManager.IsBeingSpectated)
                    Screen.SendReplayFramesToServer(true);

                if (Screen.IsPlayTesting)
                {
                    if (AudioEngine.Track.IsPlaying)
                    {
                        AudioEngine.Track.Pause();
                        AudioEngine.Track.Seek(Screen.PlayTestAudioTime);
                    }

                    Screen.Exit(() => new EditorScreen(Screen.OriginalEditorMap));
                    ResultsScreenLoadInitiated = true;
                    return;
                }

                if (Screen.IsCalibratingOffset)
                {
                    Screen.HandleSuggestedOffsetCalculations();
                    ResultsScreenLoadInitiated = true;
                    return;
                }

                // In a multiplayer match
                if (OnlineManager.CurrentGame != null)
                {
                    try
                    {
                        var playingUsers = GetScoreboardUsers();

                        var allPlayersFinished = playingUsers.All(x => x.Processor.TotalJudgementCount == Screen.Ruleset.ScoreProcessor.TotalJudgementCount);

                        if (Screen.LastJudgementIndexSentToServer == Screen.Ruleset.ScoreProcessor.TotalJudgementCount - 1 && allPlayersFinished)
                        {
                            OnlineManager.Client.FinishMultiplayerGameSession();
                            ResultsScreenLoadInitiated = true;
                        }

                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                    return;
                }

                Screen.Exit(() =>
                {
                    if (Screen.HasQuit && ConfigManager.SkipResultsScreenAfterQuit.Value)
                    {
                        if (ModManager.Mods.HasFlag(ModIdentifier.Paused))
                            ModManager.RemoveMod(ModIdentifier.Paused);

                        return new SelectScreen();
                    }


                    return new ResultScreen(Screen);
                }, 500);

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
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameEnded(object sender, GameEndedEventArgs e)
        {
            Screen.IsPaused = true;

            var screen = new MultiplayerScreen(OnlineManager.CurrentGame, true);
            Screen.Exit(() => new ResultScreen(Screen, GetScoreboardUsers(), screen));
        }

        private List<ScoreboardUser> GetScoreboardUsers()
        {
            var scoreboardUsers = new List<ScoreboardUser>();

            if (ScoreboardLeft.Users.Count != 0)
            {
                var users = ScoreboardLeft.Users.Where(x => !x.HasQuit);
                scoreboardUsers.AddRange(users);
            }

            if (ScoreboardRight != null && ScoreboardRight.Users.Count != 0)
            {
                var users = ScoreboardRight.Users.Where(x => !x.HasQuit);
                scoreboardUsers.AddRange(users);
            }

            return scoreboardUsers;
        }

        /// <summary>
        /// </summary>
        private void HandleWaitingForPlayersDialog()
        {
            if (MultiplayerEndTime == null)
                return;

            var previouslyVisible = MultiplayerEndTime.Visible;

            MultiplayerEndTime.Visible = Screen.IsPlayComplete;

            if (!previouslyVisible && MultiplayerEndTime.Visible)
            {
                MultiplayerEndTime.ClearAnimations();
                MultiplayerEndTime.Alpha = 0;
                MultiplayerEndTime.FadeTo(1, Easing.Linear, 400);
            }
        }
    }
}
