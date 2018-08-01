using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Gameplay;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Helpers;
using Quaver.Screens.Gameplay.UI;
using Quaver.Screens.Gameplay.UI.Counter;
using Quaver.Screens.Gameplay.UI.Scoreboard;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Screens.Gameplay
{
    public class GameplayScreenView : ScreenView
    {
        /// <summary>
        ///     Reference to the gameplay screen.
        /// </summary>
        public new GameplayScreen Screen { get; }

        /// <summary>
        ///     The container that will be used for displaying objects in the background.
        /// </summary>
        public Container BackgroundContainer { get; }

        /// <summary>
        ///     The background image of the map.
        /// </summary>
        private BackgroundImage Background { get; }

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
        ///     Song information displayed at the beginning of the map.
        /// </summary>
        private SongInformation SongInfo { get; set; }

        /// <summary>
        ///     The x position of the grade display
        /// </summary>
        private float GradeDisplayX => AccuracyDisplay.X - 8;

        /// <summary>
        ///     The scoreboard, lol.
        /// </summary>
        public Scoreboard Scoreboard { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public GameplayScreenView(Screen screen) : base(screen)
        {
            Screen = (GameplayScreen)screen;
            BackgroundContainer = new Container();

            // Create background on the background container
            Background = new BackgroundImage(UserInterface.MenuBackground, 70, false) {Parent = BackgroundContainer};
            CreateProgressBar();
            CreateScoreDisplay();
            CreateAccuracyDisplay();

            // Create judgement status display
            JudgementCounter = new JudgementCounter(Screen) { Parent = Container };

            CreateKeysPerSecondDisplay();
            CreateGradeDisplay();

            // Song Information Display
            SongInfo = new SongInformation(Screen)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Y = -200
            };

            CreateScoreboard();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateScoreAndAccuracyDisplays();
            GradeDisplay.X = GradeDisplayX;

            Background.Update(gameTime);
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

            BackgroundContainer.Draw(gameTime);
            Screen.Ruleset?.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundContainer.Destroy();
            Screen.Ruleset?.Destroy();
            Container?.Destroy();
        }

        /// <summary>
        ///     Creates the progress bar if the user defined it in config.
        /// </summary>
        private void CreateProgressBar()
        {
            if (!ConfigManager.DisplaySongTimeProgress.Value)
                return;

            ProgressBar = new SongTimeProgressBar(new Vector2(WindowManager.Width, 4), 0, Screen.Map.Length, 0,
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
            if (Screen.LocalScores != null)
            {
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

            Scoreboard = new Scoreboard(users) { Parent = Container };
        }

        /// <summary>
        ///     Updates the scoreboard for all the current users.
        /// </summary>
        public void UpdateScoreboardUsers() => Scoreboard.CalculateScores();
    }
}
