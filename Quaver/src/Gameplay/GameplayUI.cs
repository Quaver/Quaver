using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using Quaver.Utility;

namespace Quaver.Gameplay
{
    /// <summary>
    ///     This class Draws anything that will be shown to the player which is related to data
    /// </summary>
    internal class GameplayUI
    {
        private static Sprite AccuracyBox { get; set; }

        private static TextBoxSprite[] AccuracyIndicatorText { get; set; }

        internal static TextBoxSprite[] AccuracyCountText { get; set; }

        internal static TextBoxSprite ScoreText { get; set; }

        private static Sprite LeaderboardBox { get; set; }

        private static Boundary Boundary { get; set; }

        internal static void Initialize()
        {
            // Create Boundary
            Boundary = new Boundary();

            // Create new Accuracy Box
            AccuracyBox = new Sprite()
            {
                Alignment = Alignment.TopRight,
                Size = new Vector2(230, 240),
                Parent = Boundary,
                Tint = CustomColors.TransparentBlack //todo: remove later and use skin image
            };

            AccuracyIndicatorText = new TextBoxSprite[7];
            for (var i = 0; i < 7; i++)
            {
                AccuracyIndicatorText[i] = new TextBoxSprite()
                {
                    Parent = AccuracyBox,
                    Alignment = Alignment.TopLeft,
                    TextAlignment = Alignment.TopLeft,
                    SizeX = AccuracyBox.SizeX-20,
                    Textwrap = false,
                    Multiline = false,
                    Font = Fonts.Medium16,
                    TextColor = i == 0 ? Color.White : CustomColors.JudgeColorsTransparent[i-1],
                    Text = i == 0 ? "Accuracy" : ScoreManager.JudgeNames[i-1],
                    PositionY = i*24 + 60,
                    PositionX = 10
                };
            }

            AccuracyCountText = new TextBoxSprite[7];
            for (var i = 0; i < 7; i++)
            {
                AccuracyCountText[i] = new TextBoxSprite()
                {
                    Parent = AccuracyBox,
                    Alignment = Alignment.TopLeft,
                    TextAlignment = Alignment.TopRight,
                    SizeX = AccuracyBox.SizeX - 20,
                    Textwrap = false,
                    Multiline = false,
                    Font = Fonts.Medium16,
                    TextColor = i == 0 ? Color.White : CustomColors.JudgeColorsOpaque[i - 1],
                    Text = i == 0 ? "00.00%" : "0 | 0",
                    PositionY = AccuracyIndicatorText[i].PositionY,
                    PositionX = 10
                };
            }

            ScoreText = new TextBoxSprite()
            {
                Parent = AccuracyBox,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.MidCenter,
                SizeX = AccuracyBox.SizeX - 20,
                SizeY = 60,
                Textwrap = false,
                Multiline = false,
                Font = Fonts.Medium24,
                TextColor = Color.White,
                Text = "1234567",
                PositionY = 0,
                PositionX = 10
            };

            // Create new Leaderboard Box
            LeaderboardBox = new Sprite()
            {
                Size = new Vector2(230, 340),
                Alignment = Alignment.MidLeft,
                Parent = Boundary,
                Tint = CustomColors.TransparentBlack //todo: remove later and use skin image
            };
        }

        internal static void UpdateAccuracyBox(int index)
        {
            AccuracyCountText[index+1].Text = ScoreManager.JudgePressSpread[index] + " | " + ScoreManager.JudgeReleaseSpread[index];
            AccuracyCountText[0].Text = $"{ScoreManager.Accuracy * 100:0.00}%";
            ScoreText.Text = ScoreManager.Score.ToString();
        }

        internal static void Update(double dt)
        {
            Boundary.Update(dt);   
        }

        internal static void Draw()
        {
            Boundary.Draw();
        }

        internal static void UnloadContent()
        {
            Boundary.Destroy();
        }
    }
}
