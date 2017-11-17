using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.GameState.States;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using Quaver.Utility;

namespace Quaver.Gameplay
{
    /// <summary>
    ///     This class Draws anything that will be shown to the player which is related to data
    /// </summary>
    internal class GameplayUI : IGameplay
    {
        private Sprite AccuracyBox { get; set; }

        private Boundary[] AccuracyDisplaySet { get; set; }

        private TextBoxSprite[] AccuracyIndicatorText { get; set; }

        private TextBoxSprite[] AccuracyCountText { get; set; }

        private Sprite[] AccuracyGraphBar { get; set; }

        private float[] AccuracyGraphTargetScale { get; set; }

        private TextBoxSprite ScoreText { get; set; }

        private Sprite LeaderboardBox { get; set; }

        private Boundary Boundary { get; set; }

        public bool NoteHolding { get; set; }

        private double CurrentScore { get; set; }

        private double CurrentAccuracy { get; set; }

        internal override void Initialize(PlayScreenState playScreen)
        {
            // Reference Variables
            PlayScreen = playScreen;
            CurrentScore = 0;
            CurrentAccuracy = 0;
            NoteHolding = false;

            // Create Boundary
            Boundary = new Boundary();

            // Create Accuracy Box Variables
            AccuracyGraphTargetScale = new float[6];

            // Create new Accuracy Box
            AccuracyBox = new Sprite()
            {
                Alignment = Alignment.TopRight,
                Size = new Vector2(220, 240) * (float)GameBase.WindowYRatio,
                Position = new Vector2(-10,10),
                Parent = Boundary,
                Alpha = 0.7f,
                Tint = Color.Black //todo: remove later and use skin image
            };

            AccuracyDisplaySet = new Boundary[7];
            for (var i = 0; i < 7; i++)
            {
                AccuracyDisplaySet[i] = new Boundary()
                {
                    Parent = AccuracyBox,
                    Alignment = Alignment.TopLeft,
                    SizeX = AccuracyBox.SizeX - 10,
                    SizeY = 26 * (float)GameBase.WindowYRatio,
                    PositionY = (i * 25 + 55) * (float)GameBase.WindowYRatio,
                    PositionX = 5
                };
            }

            AccuracyGraphBar = new Sprite[6];
            for (var i = 0; i < 6; i++)
            {
                AccuracyGraphBar[i] = new Sprite()
                {
                    Parent = AccuracyDisplaySet[i+1],
                    Alignment = Alignment.MidLeft,
                    ScaleX = 0,
                    ScaleY = 1,
                    SizeY = -2,
                    Tint = CustomColors.JudgeColors[i],
                    Alpha = 0.12f
                };
            }

            AccuracyIndicatorText = new TextBoxSprite[7];
            for (var i = 0; i < 7; i++)
            {
                AccuracyIndicatorText[i] = new TextBoxSprite()
                {
                    Parent = AccuracyDisplaySet[i],
                    Alignment = Alignment.TopLeft,
                    TextAlignment = Alignment.MidLeft,
                    Scale = Vector2.One,
                    PositionX = 5,
                    Textwrap = false,
                    Multiline = false,
                    Font = Fonts.Medium16,
                    TextColor = i == 0 ? Color.White : CustomColors.JudgeColors[i-1],
                    Text = i == 0 ? "Accuracy" : PlayScreen.ScoreManager.JudgeNames[i-1],
                    Alpha = 0.3f
                };
            }

            AccuracyCountText = new TextBoxSprite[7];
            for (var i = 0; i < 7; i++)
            {
                AccuracyCountText[i] = new TextBoxSprite()
                {
                    Parent = AccuracyDisplaySet[i],
                    Alignment = Alignment.TopLeft,
                    TextAlignment = Alignment.MidRight,
                    Scale = Vector2.One,
                    PositionX = -5,
                    Textwrap = false,
                    Multiline = false,
                    Font = Fonts.Medium16,
                    TextColor = i == 0 ? Color.White : CustomColors.JudgeColors[i - 1],
                    Text = i == 0 ? "00.00%" : "0 | 0"
                };
            }

            ScoreText = new TextBoxSprite()
            {
                Parent = AccuracyBox,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.MidCenter,
                SizeX = AccuracyBox.SizeX - 20,
                SizeY = 55 * (float)GameBase.WindowYRatio,
                Textwrap = false,
                Multiline = false,
                Font = Fonts.Medium24,
                TextColor = Color.White,
                Text = "0000000",
                PositionY = 0,
                PositionX = 10
            };

            // Create new Leaderboard Box
            LeaderboardBox = new Sprite()
            {
                Size = new Vector2(230, 400),
                Alignment = Alignment.MidLeft,
                Parent = Boundary,
                Alpha = 0f,
                Tint = Color.Black //todo: remove later and use skin image
            };
        }

        internal void UpdateAccuracyBox(int index)
        {
            AccuracyCountText[index+1].Text = PlayScreen.ScoreManager.JudgePressSpread[index] + " | " + PlayScreen.ScoreManager.JudgeReleaseSpread[index];
            //AccuracyCountText[0].Text = $"{ScoreManager.Accuracy * 100:0.00}%";

            //Calculate graph bars
            for (var i = 0; i < 6; i++)
            {
                AccuracyGraphTargetScale[i] = (float)Math.Sqrt((double)(PlayScreen.ScoreManager.JudgePressSpread[i] + PlayScreen.ScoreManager.JudgeReleaseSpread[i]) / PlayScreen.ScoreManager.JudgeCount);
            }
        }

        internal override void Update(double dt)
        {
            // Update Accuracy Graph Bars
            double tween = Math.Min(dt / 100, 1);
            for (var i = 0; i < 6; i++)
            {
                AccuracyGraphBar[i].ScaleX = Util.Tween(AccuracyGraphTargetScale[i], AccuracyGraphBar[i].ScaleX, tween);
            }

            // If there's an active long note, the score will have a "slider" effect (+1 point every 25ms), otherwise it will tween normally
            if (NoteHolding)
            {
                CurrentScore += tween*4;
                if (CurrentScore > PlayScreen.ScoreManager.Score) CurrentScore = PlayScreen.ScoreManager.Score;
            }
            else
            {
                CurrentScore = PlayScreen.ScoreManager.Score; // Util.Tween(ScoreManager.Score, (float)CurrentScore, tween);
                //if (CurrentScore > ScoreManager.Score) CurrentScore = ScoreManager.Score;
            }

            // Update Score Text
            ScoreText.Text = Util.ScoreToString((int)CurrentScore);

            // Update Accuracy Text
            CurrentAccuracy = Util.Tween(PlayScreen.ScoreManager.Accuracy, (float)CurrentAccuracy, dt/10);
            AccuracyCountText[0].Text = $"{CurrentAccuracy * 100:0.00}%";

            // Update Boundary
            Boundary.Update(dt);   
        }

        internal override void Draw()
        {
            Boundary.Draw();
        }

        internal override void UnloadContent()
        {
            Boundary.Destroy();
        }
    }
}
