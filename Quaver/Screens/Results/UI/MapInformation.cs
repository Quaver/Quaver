using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Helpers;
using Quaver.Assets;
using Quaver.Graphics.Backgrounds;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Screens.Results.UI
{
    public class MapInformation : Sprite
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

        /// <summary>
        ///     The grade achieved on the play.
        /// </summary>
        private Sprite GradeImage { get; }

        /// <summary>
        ///     The spacing between the text and the background, and the text's Y spacing.
        /// </summary>
        private Vector2 TextSpacing { get; } = new Vector2(20, 25);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MapInformation(ResultsScreen screen)
        {
            Screen = screen;

            Alignment = Alignment.TopCenter;
            Size = new ScalableVector2(WindowManager.Width - 100, 125);
            Y = -Height;

            Tint = Color.Black;
            Alpha = 0.45f;

            BackgroundBorder = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(195, 110),
                X = 10,
                Tint = Color.White
            };

            Background = new Sprite()
            {
                Parent = BackgroundBorder,
                Size = new ScalableVector2(BackgroundBorder.Width - 2, BackgroundBorder.Height - 2),
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(1, 1),
                Image = BackgroundManager.Background.Image
            };

            Title = new SpriteText(Fonts.AllerRegular16, Screen.SongTitle)
            {
                Parent = this,
                X = BackgroundBorder.X + BackgroundBorder.Width,
                TextScale = 0.95f,
                Y = 25
            };

            Title.X += Title.MeasureString().X / 2 + TextSpacing.X;

            Creator = new SpriteText(Fonts.AllerRegular16, $"By: {Screen.Qua.Creator}")
            {
                Parent = this,
                X = BackgroundBorder.X + BackgroundBorder.Width,
                TextScale = 0.75f,
                Y = Title.Y + TextSpacing.Y
            };

            Creator.X += Creator.MeasureString().X / 2 + TextSpacing.X;

            Player = new SpriteText(Fonts.AllerRegular16, $"Played by: {Screen.Replay.PlayerName}")
            {
                Parent = this,
                X = BackgroundBorder.X + BackgroundBorder.Width,
                TextScale = 0.75f,
                Y = Creator.Y + TextSpacing.Y
            };

            Player.X += Player.MeasureString().X / 2 + TextSpacing.X;

            var date = Screen.GameplayScreen != null ? DateTime.Now : Screen.Replay.Date;
            Date = new SpriteText(Fonts.AllerRegular16, $"Date: {date.ToShortDateString()} @ {date:hh:mm:sstt}")
            {
                Parent = this,
                X = BackgroundBorder.X + BackgroundBorder.Width,
                TextScale = 0.75f,
                Y = Player.Y + TextSpacing.Y
            };

            Date.X += Date.MeasureString().X / 2 + TextSpacing.X;

            Texture2D gradeTexture;

            if (Screen.GameplayScreen != null && Screen.GameplayScreen.Failed)
                gradeTexture = SkinManager.Skin.Grades[API.Enums.Grade.F];
            else
                gradeTexture = SkinManager.Skin.Grades[GradeHelper.GetGradeFromAccuracy(Screen.ScoreProcessor.Accuracy)];

            GradeImage = new Sprite()
            {
                Parent = this,
                Image = gradeTexture,
                Size = new ScalableVector2(100, 100f * gradeTexture.Width / gradeTexture.Height),
                Alignment = Alignment.MidRight,
                X = -40
            };
        }
    }
}
