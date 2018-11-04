using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
using Quaver.Helpers;
using Quaver.Resources;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.SongSelect.UI.Maps
{
    public class DrawableDifficulty : Button
    {
        /// <summary>
        ///     Sprite that represents the game mode of the map.
        /// </summary>
        private Sprite Mode { get; }

        /// <summary>
        ///     The name of the difficulty.
        /// </summary>
        private SpriteText DifficultyName { get; }

        /// <summary>
        ///     The text that says "Difficulty:"
        /// </summary>
        private SpriteText TextDifficulty { get; }

        /// <summary>
        ///     Displays the actual difficulty rating.
        /// </summary>
        private SpriteText TextDifficultyRating { get; }

        /// <summary>
        ///     Displays the creator of the map.
        /// </summary>
        private SpriteText Creator { get; }

        /// <summary>
        ///     The height of the drawable mapset.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static int HEIGHT { get; } = 74;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DrawableDifficulty()
        {
            Size = new ScalableVector2(416, HEIGHT);
            Tint = Color.Black;
            Alpha = 0.85f;
            AddBorder(Color.White, 2);

            Mode = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Height * 0.70f, Height * 0.70f),
                Alignment = Alignment.MidLeft,
                X = 10,
                Y = 2,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_dot_and_circle)
            };

            DifficultyName = new SpriteText(BitmapFonts.Exo2Bold, " ", 13)
            {
                Parent = this,
                Position = new ScalableVector2(Mode.X + Mode.Width + 10, 12)
            };

            TextDifficulty = new SpriteText(BitmapFonts.Exo2SemiBold, "Difficulty:", 12, false)
            {
                Parent = this,
                Position = new ScalableVector2(DifficultyName.X, DifficultyName.Y + DifficultyName.Height + 3)
            };

            TextDifficultyRating = new SpriteText(BitmapFonts.Exo2Bold, " ", 12, false)
            {
                Parent = this,
                Position = new ScalableVector2(TextDifficulty.X + TextDifficulty.Width + 1, TextDifficulty.Y),
                Tint = ColorHelper.DifficultyToColor(19.12f)
            };

            Creator = new SpriteText(BitmapFonts.Exo2Medium, " ", 10, false)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Position = new ScalableVector2(-5, TextDifficultyRating.Y + TextDifficultyRating.Height - 4)
            };
        }

        /// <summary>
        ///     Updates the information of the map with a new one.
        /// </summary>
        /// <param name="map"></param>
        public void UpdateWithNewMap(Map map)
        {
            // TODO: Update game mode sprite.

            DifficultyName.Text = map.DifficultyName;
            TextDifficultyRating.Text = StringHelper.AccuracyToString(map.DifficultyRating).Replace("%", "");
            TextDifficultyRating.Tint = ColorHelper.DifficultyToColor(map.DifficultyRating);
            Creator.Text = $"By: {map.Creator}";
        }
    }
}