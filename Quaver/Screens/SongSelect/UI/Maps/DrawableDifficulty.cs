using System;
using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
using Quaver.Helpers;
using Quaver.Resources;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.SongSelect.UI.Maps
{
    public class DrawableDifficulty : Button
    {
        /// <summary>
        ///     Reference to the parent container.
        /// </summary>
        private DifficultyScrollContainer Container { get; }

        /// <summary>
        ///     The map this difficulty is for.
        /// </summary>
        public Map Map { get; private set; }

        /// <summary>
        ///     Sprite that represents the game mode of the map.
        /// </summary>
        private Sprite Mode { get; }

        /// <summary>
        ///     The name of the difficulty.
        /// </summary>
        private SpriteText DifficultyName { get; }

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
        public static int HEIGHT { get; } = 86;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DrawableDifficulty(DifficultyScrollContainer container)
        {
            Container = container;

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

            DifficultyName = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 13)
            {
                Parent = this,
                Position = new ScalableVector2(Mode.X + Mode.Width + 15, 12)
            };

            TextDifficultyRating = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 12)
            {
                Parent = this,
                Position = new ScalableVector2(DifficultyName.X, DifficultyName.Y + DifficultyName.Height + 4),
                Tint = ColorHelper.DifficultyToColor(19.12f)
            };

            Creator = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 10)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Position = new ScalableVector2(-5, TextDifficultyRating.Y + TextDifficultyRating.Height + 3)
            };

            Clicked += OnClicked;
        }

        /// <summary>
        ///     Updates the information of the map with a new one.
        /// </summary>
        /// <param name="map"></param>
        public void UpdateWithNewMap(Map map)
        {
            Map = map;

            // TODO: Update game mode sprite.

            DifficultyName.Text = map.DifficultyName;
            TextDifficultyRating.Text = StringHelper.AccuracyToString(map.DifficultyRating).Replace("%", "");
            TextDifficultyRating.Tint = ColorHelper.DifficultyToColor(map.DifficultyRating);
            Creator.Text = $"By: {map.Creator}";
        }

        /// <summary>
        ///     Displays the difficulty when it is selected.
        /// </summary>
        public void DisplayAsSelected()
        {
            Animations.Clear();
            ChangeWidthTo(514, Easing.OutQuint, 400);

            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, 0.85f, 400));

            DifficultyName.Animations.Clear();
            TextDifficultyRating.Animations.Clear();
            Creator.ClearAnimations();

            Border.Animations.Clear();
            Border.FadeToColor(Color.Gold, Easing.Linear, 200);
            Border.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, Border.Alpha, 1, 400));
        }

        /// <summary>
        ///     Displays the difficulty when deselected.
        /// </summary>
        public void DisplayAsDeselected()
        {
            Animations.Clear();
            ChangeWidthTo(414, Easing.OutQuint, 400);

            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, 0.50f, 400));

            DifficultyName.Animations.Clear();
            TextDifficultyRating.Animations.Clear();
            Creator.ClearAnimations();

            Border.Animations.Clear();
            Border.FadeToColor(Color.White, Easing.Linear, 200);
            Border.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, Border.Alpha, 0.65f, 400));
        }

        /// <summary>
        ///     Called when the button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            // Don't bother if the map is already selected.
            if (Map == MapManager.Selected.Value)
            {

                return;
            }

            var view = Container.View;
            view.MapsetScrollContainer.SelectMap(view.MapsetScrollContainer.SelectedMapsetIndex, Map, true);
        }
    }
}