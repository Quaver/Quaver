using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Gameplay.Mania.UI.Accuracy
{
    /// <summary>
    ///     This class Draws anything that will be shown to the player which is related to data
    /// </summary>
    internal class ManiaAccuracyBox : IGameStateComponent
    {
        /// <summary>
        ///     QuaverContainer that holds all QuaverUserInterface element for this object
        /// </summary>
        private QuaverContainer QuaverContainer { get; set; }

        /// <summary>
        ///     Text box which displays a count of every judgement
        /// </summary>
        private QuaverSpriteText[] AccuracyCountQuaverText { get; set; }

        /// <summary>
        ///     The graph which displays judgement count relative to total count of every judgement
        /// </summary>
        private QuaverSprite[] AccuracyGraphBar { get; set; }

        /// <summary>
        ///     Grade image which sits at the left side of the grade progress bar
        /// </summary>
        private QuaverSprite GradeLeft { get; set; }

        /// <summary>
        ///     Grade image which sits at the right side of the grade progress bar
        /// </summary>
        private QuaverSprite GradeRight { get; set; }

        /// <summary>
        ///     Bar which shows how far the player is from acheiving a certain grade
        /// </summary>
        private QuaverBarDisplay GradeProgressQuaverBar { get; set; }

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
        private QuaverSpriteText ScoreQuaverText { get; set; }

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
            CurrentAccuracy = 0;
            CurrentGrade = 0;
            JudgementCount = new int[6];
            ProgressBarScale = 0;

            // Create QuaverContainer
            QuaverContainer = new QuaverContainer();

            // Create Accuracy Box Variables
            AccuracyGraphTargetScale = new float[6];

            // Create new Accuracy Box
            var accuracyBox = new QuaverSprite()
            {
                Alignment = Alignment.TopRight,
                Size = new UDim2D(200 * GameBase.WindowUIScale, 240 * GameBase.WindowUIScale),
                Position = new UDim2D(-10,10),
                Parent = QuaverContainer,
                Alpha = 0.7f,
                Tint = Color.Black //todo: remove later and use skin image
            };

            var accuracyDisplaySet = new QuaverContainer[7];
            for (var i = 0; i < 7; i++)
            {
                accuracyDisplaySet[i] = new QuaverContainer()
                {
                    Parent = accuracyBox,
                    Alignment = Alignment.TopLeft,
                    Size = new UDim2D(accuracyBox.SizeX - 10, 26 * GameBase.WindowUIScale),
                    Position = new UDim2D(5, ((i * 25) + 55) * GameBase.WindowUIScale)
                };
            }

            AccuracyGraphBar = new QuaverSprite[6];
            for (var i = 0; i < 6; i++)
            {
                AccuracyGraphBar[i] = new QuaverSprite()
                {
                    Parent = accuracyDisplaySet[i+1],
                    Alignment = Alignment.MidLeft,
                    Size = new UDim2D(0, -2, 0, 1),
                    Tint = GameBase.LoadedSkin.JudgeColors[i],
                    Alpha = 0.12f
                };
            }

            var accuracyIndicatorText = new QuaverSpriteText[7];
            for (var i = 0; i < 7; i++)
            {
                accuracyIndicatorText[i] = new QuaverSpriteText()
                {
                    Parent = accuracyDisplaySet[i],
                    Alignment = Alignment.TopLeft,
                    TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                    TextAlignment = Alignment.MidLeft,
                    Size = new UDim2D(0, 0, 1, 1),
                    Position = new UDim2D(5, 0),
                    Font = QuaverFonts.Medium16,
                    TextColor = i == 0 ? Color.White : GameBase.LoadedSkin.JudgeColors[i-1],
                    Text = i == 0 ? "Accuracy" : ManiaGameplayReferences.JudgeNames[i-1],
                    TextScale = GameBase.WindowUIScale,
                    Alpha = 0.3f
                };
            }

            AccuracyCountQuaverText = new QuaverSpriteText[7];
            for (var i = 0; i < 7; i++)
            {
                AccuracyCountQuaverText[i] = new QuaverSpriteText()
                {
                    Parent = accuracyDisplaySet[i],
                    Alignment = Alignment.TopLeft,
                    TextBoxStyle = TextBoxStyle.ScaledSingleLine,
                    TextAlignment = Alignment.MidRight,
                    Size = new UDim2D(0, 0, 1, 1),
                    Position = new UDim2D(-5, 0),
                    Font = QuaverFonts.Medium16,
                    TextColor = i == 0 ? Color.White : GameBase.LoadedSkin.JudgeColors[i - 1],
                    Text = i == 0 ? "00.00%" : "0 | 0",
                    TextScale = GameBase.WindowUIScale,
                };
            }

            ScoreQuaverText = new QuaverSpriteText()
            {
                Parent = accuracyBox,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.MidCenter,
                Size = new UDim2D(accuracyBox.SizeX - 20, 55 * GameBase.WindowUIScale),
                Position = new UDim2D(10, 0),
                Font = QuaverFonts.Medium24,
                TextColor = Color.White,
                Text = "0000000",
                TextScale = GameBase.WindowUIScale,
            };

            // Create Grade box
            var gradeBox = new QuaverContainer()
            {
                Parent = accuracyBox,
                Size = new UDim2D(accuracyBox.SizeX, 26 * GameBase.WindowUIScale),
                Position = new UDim2D(0, 31 * GameBase.WindowUIScale),
                Alignment = Alignment.BotLeft,
            };

            GradeProgressQuaverBar = new QuaverBarDisplay(GameBase.WindowUIScale, accuracyBox.SizeX - (gradeBox.SizeY * 2) - 30 * GameBase.WindowUIScale, new Color[] { Color.Red })
            {
                Parent = gradeBox,
                Alignment = Alignment.MidCenter
            };

            GradeLeft = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.GradeSmallF,
                Size = new UDim2D(gradeBox.SizeY * GameBase.WindowUIScale, gradeBox.SizeY * GameBase.WindowUIScale),
                //PositionX = GradeProgressQuaverBar.PositionX - 32 * GameBase.WindowUIScale,
                Parent = gradeBox
            };

            GradeRight = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.GradeSmallD,
                Size = new UDim2D(gradeBox.SizeY * GameBase.WindowUIScale, gradeBox.SizeY * GameBase.WindowUIScale),
                Alignment = Alignment.TopRight,
                //PositionX = GradeProgressQuaverBar.PositionX + GradeProgressQuaverBar.SizeX + 32 * GameBase.WindowUIScale,
                Parent = gradeBox
            };

            // todo: Create new Leaderboard Box
            /*
            LeaderboardBox = new QuaverSprite()
            {
                Size = new UDim2D(230, 400),
                Alignment = Alignment.MidLeft,
                Parent = QuaverContainer,
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
            //CurrentScore = totalScore;
            ScoreQuaverText.Text = StringHelper.ScoreToString(totalScore);
            TargetAccuracy = (float)tarAcc;
            JudgementCount[index] = pressSpread + releaseSpread;
            AccuracyCountQuaverText[index+1].Text = pressSpread + " | " + releaseSpread;

            //Calculate graph bars
            AccuracyGraphTargetScale[index] = (float)Math.Sqrt((double)(JudgementCount[index]) / judgeCount);
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
                GradeProgressQuaverBar.UpdateBar(0, scale,
                QuaverColors.GradeColors[CurrentGrade + 1]);
            }
        }

        public void Update(double dt)
        {
            // Update Accuracy Graph Bars
            double tween = Math.Min(dt / 30, 1);
            for (var i = 0; i < 6; i++)
            {
                AccuracyGraphBar[i].ScaleX = GraphicsHelper.Tween(AccuracyGraphTargetScale[i], AccuracyGraphBar[i].ScaleX, tween);
            }

            // Update Score Text
            //ScoreQuaverText.Text = GraphicsHelper.ScoreToString((int)CurrentScore);

            // Update Accuracy Text
            CurrentAccuracy = GraphicsHelper.Tween(TargetAccuracy, CurrentAccuracy, tween);
            AccuracyCountQuaverText[0].Text = $"{CurrentAccuracy * 100:0.00}%";

            // Upgrade Grade Progress Bar
            GradeProgressQuaverBar.UpdateBar(0,
                GraphicsHelper.Tween(ProgressBarScale, GradeProgressQuaverBar.GetBarScale(0), tween));

            // Update QuaverContainer
            QuaverContainer.Update(dt);   
        }

        public void Draw()
        {
            QuaverContainer.Draw();
        }

        public void UnloadContent()
        {
            QuaverContainer.Destroy();
        }
    }
}
