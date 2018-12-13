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
        public const float HEIGHT = 60;
        public const float WIDTH = 160;
        public const int TOTAL_DATA_SIZE = 32;

        private float MaxValue { get; set; }

        private float CurrentCoefficient { get; set; }

        private float CoefficientOffset { get; set; }

        private SpriteText DataText { get; set; }

        private List<Sprite> DataSprite { get; set; }

        private float[] DataSet { get; set; }

        public ProfilerGraph(float coefficientOffset, Color color)
        {
            CoefficientOffset = coefficientOffset;

            Width = WIDTH;
            Height = HEIGHT;
            Tint = Color.Black;
            Alpha = 0.5f;

            DataSprite = new List<Sprite>();
            DataSet = new float[TOTAL_DATA_SIZE];
            var dataSpriteSize = (WIDTH / TOTAL_DATA_SIZE);
            for (var i = 0; i < TOTAL_DATA_SIZE; i++)
            {
                DataSprite.Add(new Sprite()
                {
                    Parent = this,
                    Alignment = Alignment.BotLeft,
                    Position = new ScalableVector2(i * dataSpriteSize, 0.5f),
                    Size = new ScalableVector2(dataSpriteSize, 0),
                    Tint = color
                });
            }

            DataText = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 12)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(2, 2),
                WidthScale = 1,
                Height = 20
            };
        }

        public void UpdateData(float current, string text)
        {
            CurrentCoefficient = 1;
            for (var i = TOTAL_DATA_SIZE - 1; i > 0; i--)
            {
                DataSet[i] = DataSet[i - 1];
                if (DataSet[i] > CurrentCoefficient)
                    CurrentCoefficient = DataSet[i];
            }
            DataSet[0] = current;
            if (DataSet[0] > CurrentCoefficient)
                CurrentCoefficient += DataSet[0];

            CurrentCoefficient += CoefficientOffset;

            for (var i = 0; i < TOTAL_DATA_SIZE; i++)
            {
                DataSprite[i].Height = (DataSet[i] / CurrentCoefficient) * HEIGHT;
            }

            DataText.Text = text;
        }
    }
}
