using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Metadata;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components
{
    public class DrawableMapTextDifficultyRating : SpriteTextPlus, IDrawableMapMetadata
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
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e) => UpdateText();

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
    }
}