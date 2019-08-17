using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Metadata;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components
{
    public class DrawableMapTextDifficultyName : SpriteTextPlus, IDrawableMapMetadata
    {
        /// <summary>
        /// </summary>
        private DrawableMap Map { get; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        public DrawableMapTextDifficultyName(DrawableMap map) : base(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
        {
            Map = map;
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
        private void OnModsChanged(object sender, ModsChangedEventArgs e)
            => Tint = ColorHelper.DifficultyToColor((float) Map.Map.DifficultyFromMods(ModManager.Mods));

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
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