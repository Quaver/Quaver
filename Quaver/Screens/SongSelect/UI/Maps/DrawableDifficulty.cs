using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Helpers;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Screens.Loading;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

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
        public static int HEIGHT { get; } = 92;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DrawableDifficulty(DifficultyScrollContainer container)
        {
            Container = container;

            Size = new ScalableVector2(416, HEIGHT);
            Image = UserInterface.SelectButtonBackground;

            DifficultyName = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 13)
            {
                Parent = this,
                Position = new ScalableVector2(15, 12)
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
            ChangeWidthTo(574, Easing.OutQuint, 400);
            FadeToColor(Colors.MainAccent, Easing.OutQuint, 300);

            DifficultyName.Animations.Clear();
            TextDifficultyRating.Animations.Clear();
            Creator.ClearAnimations();

            DifficultyName.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, DifficultyName.Alpha, 1f, 400));
            TextDifficultyRating.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, TextDifficultyRating.Alpha, 1f, 400));
            Creator.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, Creator.Alpha, 1f, 400));
        }

        /// <summary>
        ///     Displays the difficulty when deselected.
        /// </summary>
        public void DisplayAsDeselected()
        {
            Animations.Clear();
            ChangeWidthTo(414, Easing.OutQuint, 400);
            FadeToColor(Color.Black, Easing.OutQuint, 300);

            DifficultyName.Animations.Clear();
            TextDifficultyRating.Animations.Clear();
            Creator.ClearAnimations();

            DifficultyName.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, DifficultyName.Alpha, 0.65f, 400));
            TextDifficultyRating.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, TextDifficultyRating.Alpha, 0.65f, 400));
            Creator.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, Creator.Alpha, 0.65f, 400));
        }

        /// <summary>
        ///     Called when the button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            if (Map == MapManager.Selected.Value)
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game.CurrentScreen as SongSelectScreen;
                screen?.ExitToGameplay();
                return;
            }

            var view = Container.View;
            view.MapsetScrollContainer.SelectMap(view.MapsetScrollContainer.SelectedMapsetIndex, Map, true);
        }

        /// <inheritdoc />
        /// <summary>
        ///     In this case, we only want buttons to be clickable if they're in the bounds of the scroll container.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = Rectangle.Intersect(ScreenRectangle.ToRectangle(), Container.ScreenRectangle.ToRectangle());
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }
    }
}