using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Gameplay.UI.Judgements;

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
        private QuaverContainer Container { get; }

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
        private JudgementStatusDisplay JudgementStatusDisplay { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        internal GameplayInterface(GameplayScreen screen)
        {
            Screen = screen;
            Container = new QuaverContainer();
        }
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            // Initialize the progress bar if the user has it set in config.
            if (ConfigManager.DisplaySongTimeProgress.Value)
                SongTimeProgressBar = new SongTimeProgressBar(Qua.FindSongLength(GameBase.SelectedMap.Qua), 0, new Vector2(GameBase.WindowRectangle.Width, 6),
                                                            Container, Alignment.BotLeft);

            // Create score display
            ScoreDisplay = new NumberDisplay(NumberDisplayType.Score, StringHelper.ScoreToString(0))
            {
                Parent = Container,
                Alignment = Alignment.TopRight
            };

            // Put the display in the top right corner.
            ScoreDisplay.PosX = -ScoreDisplay.TotalWidth - 10;
            
            // Create acc display
            AccuracyDisplay = new NumberDisplay(NumberDisplayType.Accuracy, StringHelper.AccuracyToString(0))
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
            };
            
            // Set the position of the accuracy display.
            AccuracyDisplay.PosX = -AccuracyDisplay.TotalWidth - 10;
            AccuracyDisplay.PosY = ScoreDisplay.Digits[0].SizeY + 10;
            
            // Create judgement status display
            JudgementStatusDisplay = new JudgementStatusDisplay(Screen) { Parent = Container };
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnloadContent()
        {
            Container.Destroy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            UpdateSongProgressDisplay();
            UpdateScoreAndAccuracyDisplays();
                    
            Container.Update(dt);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            Container.Draw();
        }
        
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
                AccuracyDisplay.PosX = -AccuracyDisplay.TotalWidth - 10;
        }

        /// <summary>
        ///     Updates the song progress display.
        /// </summary>
        private void UpdateSongProgressDisplay()
        {
            // Update the current value of the song time progress bar if it is actually initialized
            // and the user wants to actually display it.
            if (ConfigManager.DisplaySongTimeProgress.Value && SongTimeProgressBar != null)
                SongTimeProgressBar.CurrentValue = (float) Screen.AudioTiming.CurrentTime;
        }
    }
}