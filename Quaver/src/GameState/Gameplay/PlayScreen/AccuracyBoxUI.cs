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
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    /// <summary>
    ///     This class Draws anything that will be shown to the player which is related to data
    /// </summary>
    internal class AccuracyBoxUI : IHelper
    {
        /// <summary>
        ///     Boundary that holds all UI element for this object
        /// </summary>
        private Boundary Boundary { get; set; }

        /// <summary>
        ///     Text box which displays a count of every judgement
        /// </summary>
        private TextBoxSprite[] AccuracyCountText { get; set; }

        /// <summary>
        ///     The graph which displays judgement count relative to total count of every judgement
        /// </summary>
        private Sprite[] AccuracyGraphBar { get; set; }

        /// <summary>
        ///     Grade image which sits at the left side of the grade progress bar
        /// </summary>
        private Sprite GradeLeft { get; set; }

        /// <summary>
        ///     Grade image which sits at the right side of the grade progress bar
        /// </summary>
        private Sprite GradeRight { get; set; }

        /// <summary>
        ///     Bar which shows how far the player is from acheiving a certain grade
        /// </summary>
        private BarDisplay GradeProgressBar { get; set; }

        /// <summary>
        ///     Counts the number of total judgement for each hit window to display
        /// </summary>
        private int[] JudgementCount { get; set; }

        /// <summary>
        ///     Target size for each accuracy graph sprite
        /// </summary>
        private float[] AccuracyGraphTargetScale { get; set; }

        /// <summary>
        ///     Text which displays current score
        /// </summary>
        private TextBoxSprite ScoreText { get; set; }

        /// <summary>
        ///     Current score the player has. Used for animation/UI.
        /// </summary>
        private double CurrentScore { get; set; }

        /// <summary>
        ///     Current accuracy the player has. Used for animation
        /// </summary>
        private float CurrentAccuracy { get; set; }

        /// <summary>
        ///     The text that will be used to display accuracy. Determined by animation
        /// </summary>
        private float TargetAccuracy { get; set; }

        /// <summary>
        ///     Current grade the player has achieved
        /// </summary>
        private int CurrentGrade { get; set; }

        /// <summary>
        ///     Current size of the grade progress bar
        /// </summary>
        private float ProgressBarScale { get; set; }

        /// <summary>
        ///     Small Grade Images used for grade bar
        /// </summary>
        private Texture2D[] GradeImages { get; } = new Texture2D[9] {GameBase.LoadedSkin.GradeSmallF, GameBase.LoadedSkin.GradeSmallD, GameBase.LoadedSkin.GradeSmallC, GameBase.LoadedSkin.GradeSmallB,
                                                                        GameBase.LoadedSkin.GradeSmallA, GameBase.LoadedSkin.GradeSmallS, GameBase.LoadedSkin.GradeSmallSS, GameBase.LoadedSkin.GradeSmallX, GameBase.LoadedSkin.GradeSmallXX};

        public void Initialize(IGameState state)
        {
            // Reference Variables
            CurrentScore = 0;
            CurrentAccuracy = 0;
            CurrentGrade = 0;
            JudgementCount = new int[6];
            ProgressBarScale = 0;

            // Create Boundary
            Boundary = new Boundary();

            // Create Accuracy Box Variables
            AccuracyGraphTargetScale = new float[6];

            // Create new Accuracy Box
            var accuracyBox = new Sprite()
            {
                Alignment = Alignment.TopRight,
                Size = new UDim2(200 * GameBase.WindowUIScale, 240 * GameBase.WindowUIScale),
                Position = new UDim2(-10,10),
                Parent = Boundary,
                Alpha = 0.7f,
                Tint = Color.Black //todo: remove later and use skin image
            };

            var accuracyDisplaySet = new Boundary[7];
            for (var i = 0; i < 7; i++)
            {
                accuracyDisplaySet[i] = new Boundary()
                {
                    Parent = accuracyBox,
                    Alignment = Alignment.TopLeft,
                    Size = new UDim2(accuracyBox.SizeX - 10, 26 * GameBase.WindowUIScale),
                    Position = new UDim2(5, ((i * 25) + 55) * GameBase.WindowUIScale)
                };
            }

            AccuracyGraphBar = new Sprite[6];
            for (var i = 0; i < 6; i++)
            {
                AccuracyGraphBar[i] = new Sprite()
                {
                    Parent = accuracyDisplaySet[i+1],
                    Alignment = Alignment.MidLeft,
                    Size = new UDim2(0, -2, 0, 1),
                    Tint = GameColors.JudgeColors[i],
                    Alpha = 0.12f
                };
            }

            var accuracyIndicatorText = new TextBoxSprite[7];
            for (var i = 0; i < 7; i++)
            {
                accuracyIndicatorText[i] = new TextBoxSprite()
                {
                    Parent = accuracyDisplaySet[i],
                    Alignment = Alignment.TopLeft,
                    TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                    TextAlignment = Alignment.MidLeft,
                    Size = new UDim2(0, 0, 1, 1),
                    Position = new UDim2(5, 0),
                    Font = Fonts.Medium16,
                    TextColor = i == 0 ? Color.White : GameColors.JudgeColors[i-1],
                    Text = i == 0 ? "Accuracy" : GameplayReferences.JudgeNames[i-1],
                    TextScale = GameBase.WindowUIScale,
                    Alpha = 0.3f
                };
            }

            AccuracyCountText = new TextBoxSprite[7];
            for (var i = 0; i < 7; i++)
            {
                AccuracyCountText[i] = new TextBoxSprite()
                {
                    Parent = accuracyDisplaySet[i],
                    Alignment = Alignment.TopLeft,
                    TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                    TextAlignment = Alignment.MidRight,
                    Size = new UDim2(0, 0, 1, 1),
                    Position = new UDim2(-5, 0),
                    Font = Fonts.Medium16,
                    TextColor = i == 0 ? Color.White : GameColors.JudgeColors[i - 1],
                    Text = i == 0 ? "00.00%" : "0 | 0",
                    TextScale = GameBase.WindowUIScale,
                };
            }

            ScoreText = new TextBoxSprite()
            {
                Parent = accuracyBox,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.MidCenter,
                Size = new UDim2(accuracyBox.SizeX - 20, 55 * GameBase.WindowUIScale),
                Position = new UDim2(10, 0),
                Font = Fonts.Medium24,
                TextColor = Color.White,
                Text = "0000000",
                TextScale = GameBase.WindowUIScale,
            };

            // Create Grade box
            var gradeBox = new Boundary()
            {
                Parent = accuracyBox,
                Size = new UDim2(accuracyBox.SizeX, 26 * GameBase.WindowUIScale),
                Position = new UDim2(0, 31 * GameBase.WindowUIScale),
                Alignment = Alignment.BotLeft,
            };

            GradeProgressBar = new BarDisplay(GameBase.WindowUIScale, accuracyBox.SizeX - (gradeBox.SizeY * 2) - 30 * GameBase.WindowUIScale, new Color[] { Color.Red })
            {
                Parent = gradeBox,
                Alignment = Alignment.MidCenter
            };

            GradeLeft = new Sprite()
            {
                Image = GameBase.LoadedSkin.GradeSmallF,
                Size = new UDim2(gradeBox.SizeY * GameBase.WindowUIScale, gradeBox.SizeY * GameBase.WindowUIScale),
                //PositionX = GradeProgressBar.PositionX - 32 * GameBase.WindowUIScale,
                Parent = gradeBox
            };

            GradeRight = new Sprite()
            {
                Image = GameBase.LoadedSkin.GradeSmallD,
                Size = new UDim2(gradeBox.SizeY * GameBase.WindowUIScale, gradeBox.SizeY * GameBase.WindowUIScale),
                Alignment = Alignment.TopRight,
                //PositionX = GradeProgressBar.PositionX + GradeProgressBar.SizeX + 32 * GameBase.WindowUIScale,
                Parent = gradeBox
            };

            // todo: Create new Leaderboard Box
            /*
            LeaderboardBox = new Sprite()
            {
                Size = new UDim2(230, 400),
                Alignment = Alignment.MidLeft,
                Parent = Boundary,
                Alpha = 0f,
                Tint = Color.Black //todo: remove later and use skin image
            };*/
        }

        /// <summary>
        ///     Update the accuracy box text and graph. (The box on the top right of the playscreen.)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pressSpread"></param>
        /// <param name="releaseSpread"></param>
        /// <param name="judgeCount"></param>
        internal void UpdateAccuracyBox(int index, int pressSpread, int releaseSpread, int judgeCount, int totalScore, double tarAcc)
        {
            // Update Variables and Text
            CurrentScore = totalScore;
            TargetAccuracy = (float)tarAcc;
            JudgementCount[index] = pressSpread + releaseSpread;
            AccuracyCountText[index+1].Text = pressSpread + " | " + releaseSpread;

            //Calculate graph bars
            for (var i = 0; i < 6; i++)
            {
                AccuracyGraphTargetScale[i] = (float)Math.Sqrt((double)(JudgementCount[i]) / judgeCount);
            }
        }

        internal void UpdateGradeBar(int index, float scale)
        {
            //Update BarScale Animation
            ProgressBarScale = scale;

            //If the player got a different grade
            if (CurrentGrade != index)
            {
                //Upgrade Bar Images
                CurrentGrade = index;
                GradeLeft.Image = GradeImages[CurrentGrade + 1];
                GradeRight.Image = GradeImages[CurrentGrade + 2];

                //Upgrade Bar Color and Size
                GradeProgressBar.UpdateBar(0, scale,
                GameColors.GradeColors[CurrentGrade + 1]);
            }
        }

        public void Update(double dt)
        {
            // Update Accuracy Graph Bars
            double tween = Math.Min(dt / 50, 1);
            for (var i = 0; i < 6; i++)
            {
                AccuracyGraphBar[i].ScaleX = Util.Tween(AccuracyGraphTargetScale[i], AccuracyGraphBar[i].ScaleX, tween);
            }

            // If there's an active long note, the score will have a "slider" effect (+1 point every 25ms), otherwise it will tween normally
            /*
            if (NoteHolding)
            {
                CurrentScore += tween*2;
                if (CurrentScore > GameplayReferences.ScoreTotal) CurrentScore = GameplayReferences.ScoreTotal;
            }
            else
            {
                CurrentScore = GameplayReferences.ScoreTotal; // Util.Tween(Score, (float)CurrentScore, tween);
                //if (CurrentScore > Score) CurrentScore = Score;
            }*/

            // Update Score Text
            ScoreText.Text = Util.ScoreToString((int)CurrentScore);

            // Update Accuracy Text
            CurrentAccuracy = Util.Tween(TargetAccuracy, CurrentAccuracy, tween);
            AccuracyCountText[0].Text = $"{CurrentAccuracy * 100:0.00}%";

            // Upgrade Grade Progress Bar
            GradeProgressBar.UpdateBar(0,
                Util.Tween(ProgressBarScale, GradeProgressBar.GetBarScale(0), tween));

            // Update Boundary
            Boundary.Update(dt);   
        }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void UnloadContent()
        {
            Boundary.Destroy();
        }
    }
}
