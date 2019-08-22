using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components
{
    public class DrawableMapTextDifficultyRating : SpriteTextPlus, IDrawableMapComponent
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        public DrawableMapTextDifficultyRating(Map map) : base(FontManager.GetWobbleFont(Fonts.LatoBlack), "0.00", 22)
        {
            Map = map;

            UpdateText();
            ModManager.ModsChanged += OnModsChanged;
            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;

            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Updates the text content and color with the updated state
        /// </summary>
        public void UpdateText()
        {
            var difficulty = Map.DifficultyFromMods(ModManager.Mods);

            Text = StringHelper.RatingToString(difficulty);
            Tint = ColorHelper.DifficultyToColor((float) difficulty);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Open()
        {
            ClearAnimations();
            Wait(200);
            FadeTo(1, Easing.Linear, 250);

            if (MapManager.Selected.Value == Map)
                MoveToY(-18, Easing.Linear, 20);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Close()
        {
            ClearAnimations();
            Alpha = 0;
            Visible = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e) => UpdateText();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (MapManager.Selected.Value == Map)
                Y = -18;
            else
                Y = 0;
        }
    }
}