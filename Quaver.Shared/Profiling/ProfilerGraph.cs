using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Wobble.Graphics.Sprites;
using Wobble.Graphics;
using Quaver.Shared.Assets;

namespace Quaver.Shared.Profiling
{
    public class ProfilerGraph : Sprite
    {
        /// <summary>
        ///     Fixed Height of this object
        /// </summary>
        public const float HEIGHT = 60;

        /// <summary>
        ///     Fixed Width of this object
        /// </summary>
        public const float WIDTH = 160;

        /// <summary>
        ///     Total Dataset Size
        /// </summary>
        public const int TOTAL_DATA_SIZE = 32;

        /// <summary>
        ///     Previous Coefficient Value for Graph Sprites
        /// </summary>
        private float PreviousCoefficient { get; set; }

        /// <summary>
        ///     Current Value which will be divided by DataSet values to get Graph Sprite Size
        /// </summary>
        private float CurrentCoefficient { get; set; }

        /// <summary>
        ///     Fixed Interval Offset value used to determine Graph Height
        /// </summary>
        private float CoefficientOffset { get; set; }

        /// <summary>
        ///     Displays Data informtation
        /// </summary>
        private SpriteText DataText { get; set; }

        /// <summary>
        ///     Displays the graph via Spries
        /// </summary>
        private List<Sprite> DataSprite { get; set; }

        /// <summary>
        ///     Data Cache used for the graph
        /// </summary>
        private float[] DataSet { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="coefficientOffset"></param>
        /// <param name="color"></param>
        public ProfilerGraph(float coefficientOffset, Color color)
        {
            // Set Coefficient Value (Used for graph scaling)
            CoefficientOffset = coefficientOffset;

            // Update Properties
            Width = WIDTH;
            Height = HEIGHT;
            Tint = Color.Black;
            Alpha = 0.5f;

            // Create Data Sprites
            DataSprite = new List<Sprite>();
            DataSet = new float[TOTAL_DATA_SIZE];
            var dataSpriteSize = (WIDTH / TOTAL_DATA_SIZE);
            for (var i = 0; i < TOTAL_DATA_SIZE; i++)
            {
                DataSprite.Add(new Sprite()
                {
                    Parent = this,
                    Alignment = Alignment.BotLeft,
                    Position = new ScalableVector2(i * dataSpriteSize, 1),
                    Size = new ScalableVector2(dataSpriteSize, 0),
                    Tint = color
                });
            }

            // Create Data Text
            DataText = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 12)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(2, 2),
                WidthScale = 1,
                Height = 20
            };
        }

        /// <summary>
        ///     Updates the Data and graph display
        /// </summary>
        /// <param name="current"></param>
        /// <param name="text"></param>
        public void UpdateData(float current, string text)
        {
            // Update Dataset
            CurrentCoefficient = 1;
            for (var i = TOTAL_DATA_SIZE - 1; i > 0; i--)
            {
                DataSet[i] = DataSet[i - 1];
                if (DataSet[i] > CurrentCoefficient)
                    CurrentCoefficient = DataSet[i];
            }
            DataSet[0] = current;
            if (DataSet[0] > CurrentCoefficient)
                CurrentCoefficient = DataSet[0];

            // Update Coefficient (used for graph scaling)
            CurrentCoefficient += CoefficientOffset;
            CurrentCoefficient += (PreviousCoefficient - CoefficientOffset)/2;
            PreviousCoefficient = CurrentCoefficient;

            // Update UI
            DataText.Text = text;
            for (var i = 0; i < TOTAL_DATA_SIZE; i++)
                DataSprite[i].Height = (DataSet[i] / CurrentCoefficient) * HEIGHT;
        }
    }
}
