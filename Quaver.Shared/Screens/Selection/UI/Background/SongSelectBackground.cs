using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Skinning;
using Wobble.Bindables;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;

namespace Quaver.Shared.Screens.Selection.UI.Background
{
    public class SongSelectBackground : BackgroundImage
    {
        public SongSelectBackground() : base(UserInterface.TrianglesWallpaper, 0, false)
        {
            MapManager.Selected.ValueChanged += OnMapChanged;
            BackgroundHelper.Loaded += OnBackgroundLoaded;
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            BackgroundHelper.Loaded -= OnBackgroundLoaded;

            base.Destroy();
        }

        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (SkinManager.Skin == null || !SkinManager.Skin.SongSelect.DisplayMapBackground)
            {
                Image = UserInterface.TrianglesWallpaper;
                BrightnessSprite.ClearAnimations();
                BrightnessSprite.FadeTo(0, Easing.Linear, 250);
                return;
            }

            if (MapManager.GetBackgroundPath(e.Value) == MapManager.GetBackgroundPath(e.OldValue))
                return;

            BrightnessSprite.ClearAnimations();
            BrightnessSprite.FadeTo(1, Easing.Linear, 250);
        }

        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            if (SkinManager.Skin == null || !SkinManager.Skin.SongSelect.DisplayMapBackground)
                return;

            if (e.Map != MapManager.Selected.Value)
                return;

            Image = e.Texture;
            BrightnessSprite.ClearAnimations();

            var brightness = SkinManager.Skin?.SongSelect?.MapBackgroundBrightness ?? 15;
            BrightnessSprite.FadeTo((100 - brightness) / 100f, Easing.Linear, 250);
        }
    }
}