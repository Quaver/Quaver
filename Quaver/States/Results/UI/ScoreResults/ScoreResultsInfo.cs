using System;
using System.Collections.Generic;
using System.Linq;
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
        ///
        /// </summary>
        private List<ScoreResultsInfoItem> ResultValueItems { get; }

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

            var mods = Screen.ScoreProcessor.Mods.ToString();

            ResultValueItems = new List<ScoreResultsInfoItem>
            {
                new ScoreResultsInfoItem("Mods", mods == "0" ? "None" : mods),
                new ScoreResultsInfoItem("Score", Screen.ScoreProcessor.Score.ToString("N0")),
                new ScoreResultsInfoItem("Accuracy", StringHelper.AccuracyToString(Screen.ScoreProcessor.Accuracy)),
                new ScoreResultsInfoItem("Max Combo", Screen.ScoreProcessor.MaxCombo.ToString("N0")),
                new ScoreResultsInfoItem("Score Rating"),
                new ScoreResultsInfoItem("Map Rank"),
                new ScoreResultsInfoItem("Ranks Gained")
            };

            ResultValueItems.ForEach(x =>
            {
                if (ResultValueItems.FindAll(y => y.Title == x.Title).ToList().Count > 1)
                    throw new ArgumentException("ResultValueItems must not contain duplicate items with the same `Title.`");
            });

            InitializeItems();
        }

        /// <summary>
        ///    Initialize all of the result value items.
        /// </summary>
        private void InitializeItems()
        {
            for (var i = 0; i < ResultValueItems.Count; i++)
            {
                var item = ResultValueItems[i];

                // Calculate x position
                var sizePer = SizeX / ResultValueItems.Count;
                var posX =  sizePer * i + sizePer / 2f;

                item.Initialize(this, posX);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            ResultValueItems.ForEach(x => x.Update(dt));

            base.Update(dt);
        }
    }
}