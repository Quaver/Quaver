using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class DrawableMapMetadataDifficulty : DrawableMapMetadata
    {
        public DrawableMapMetadataDifficulty(Map map) : base(map, FontAwesome.Get(FontAwesomeIcon.fa_stethoscope))
        {
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
        private void UpdateText()
        {
            var rating = Map.DifficultyFromMods(ModManager.Mods);

            Text.Text = StringHelper.RatingToString(rating);
            Text.Tint = ColorHelper.DifficultyToColor((float) rating);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e) => UpdateText();
    }
}