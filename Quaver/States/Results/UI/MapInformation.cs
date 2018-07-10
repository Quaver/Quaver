using System;
using System.Drawing;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Gameplay;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.States.Results.UI
{
    internal class MapInformation : Sprite
    {
        /// <summary>
        ///     Reference to the results screen itself.
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///     The background for the map.
        /// </summary>
        private Sprite Background { get; }

        /// <summary>
        ///     The border for the background.
        /// </summary>
        private Sprite BackgroundBorder { get; }

        /// <summary>
        ///     The title of the song.
        /// </summary>
        private SpriteText Title { get; }

        /// <summary>
        ///     The creator of the map
        /// </summary>
        private SpriteText Creator { get; }

        /// <summary>
        ///     The player of this score
        /// </summary>
        private SpriteText Player { get; }


        /// <summary>
        ///     The date played.
        /// </summary>
        private SpriteText Date { get; }

        private Sprite GradeImage { get; }

        /// <summary>
        ///     The spacing between the text and the background, and the text's Y spacing.
        /// </summary>
        private Vector2 TextSpacing { get; } = new Vector2(20, 25);

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal MapInformation(ResultsScreen screen)
        {
            Screen = screen;

            Alignment = Alignment.TopCenter;
            Size = new UDim2D(GameBase.WindowRectangle.Width - 100, 125);

            Tint = Color.Black;
            Alpha = 0.45f;

            BackgroundBorder = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new UDim2D(195, 110),
                PosX = 10,
                Tint = Color.White
            };

            Background = new Sprite
            {
                Parent = BackgroundBorder,
                Size = new UDim2D(BackgroundBorder.SizeX - 2, BackgroundBorder.SizeY - 2),
                Alignment = Alignment.TopLeft,
                Position = new UDim2D(1, 1),
                Image = GameBase.QuaverUserInterface.MenuBackground
            };

            Title = new SpriteText
            {
                Parent = this,
                Text = $"{Screen.Qua.Artist} - {Screen.Qua.Title} [{Screen.Qua.DifficultyName}]",
                Font = Fonts.AllerRegular16,
                PosX = BackgroundBorder.PosX  + BackgroundBorder.SizeX,
                TextScale = 0.95f,
                PosY = 25
            };

            Title.PosX += Title.MeasureString().X / 2 + TextSpacing.X;

            Creator = new SpriteText()
            {
                Parent = this,
                Text = $"By: {Screen.Qua.Creator}",
                Font = Fonts.AllerRegular16,
                PosX = BackgroundBorder.PosX + BackgroundBorder.SizeX,
                TextScale = 0.75f,
                PosY = Title.PosY + TextSpacing.Y
            };

            Creator.PosX += Creator.MeasureString().X / 2 + TextSpacing.X;

            Player = new SpriteText()
            {
                Parent = this,
                Text = $"Played by: {Screen.Replay.PlayerName}",
                Font = Fonts.AllerRegular16,
                PosX = BackgroundBorder.PosX + BackgroundBorder.SizeX,
                TextScale = 0.75f,
                PosY = Creator.PosY + TextSpacing.Y
            };

            Player.PosX += Player.MeasureString().X / 2 + TextSpacing.X;

            var date = Screen.GameplayScreen != null ? DateTime.Now : Screen.Replay.Date;
            Date = new SpriteText
            {
                Parent = this,
                Text = $"Date: {date.ToShortDateString()} @ {date:hh:mm:sstt}",
                Font = Fonts.AllerRegular16,
                PosX = BackgroundBorder.PosX + BackgroundBorder.SizeX,
                TextScale = 0.75f,
                PosY = Player.PosY + TextSpacing.Y
            };

            Date.PosX += Date.MeasureString().X / 2 + TextSpacing.X;

            Texture2D gradeTexture;

            if (Screen.GameplayScreen != null && Screen.GameplayScreen.Failed)
                gradeTexture = GameBase.Skin.Grades[API.Enums.Grade.F];
            else
                gradeTexture = GameBase.Skin.Grades[GradeHelper.GetGradeFromAccuracy(Screen.ScoreProcessor.Accuracy)];

            GradeImage = new Sprite()
            {
                Parent = this,
                Image = gradeTexture,
                Size = new UDim2D(100, 100f * gradeTexture.Width / gradeTexture.Height),
                Alignment = Alignment.MidRight,
                PosX = -40
            };
        }
    }
}