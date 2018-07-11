using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Results.UI.ScoreResults
{
    internal class ScoreResultsInfo : Sprite
    {
        /// <summary>
        ///     Reference to the results screen itself
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///    Dictionary containing the result value's "title" and value
        /// </summary>
        private Dictionary<string, string> ResultValue { get; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="screen"></param>
        internal ScoreResultsInfo(ResultsScreen screen)
        {
            Screen = screen;

            Alignment = Alignment.TopCenter;
            Size = new UDim2D(GameBase.WindowRectangle.Width - 100, 75);

            Tint = Color.Black;
            Alpha = 0.35f;

            ResultValue = new Dictionary<string, string>
            {
                {"Score", Screen.ScoreProcessor.Score.ToString()},
                {"Accuracy", StringHelper.AccuracyToString(Screen.ScoreProcessor.Accuracy)},
                {"Max Combo", Screen.ScoreProcessor.MaxCombo.ToString()},
                {"Score Rating", "-1"},
                {"Map Rank", "-1"},
                {"Ranks Gained", "-1"},
                {"XP Left To Level 0", "-1"}
            };
        }
    }
}