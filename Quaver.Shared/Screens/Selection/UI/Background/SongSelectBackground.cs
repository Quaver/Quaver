using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
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
            if (ConfigManager.DisplayMapBackgroundInSelect == null || !ConfigManager.DisplayMapBackgroundInSelect.Value)
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
            if (ConfigManager.DisplayMapBackgroundInSelect == null || !ConfigManager.DisplayMapBackgroundInSelect.Value)
                return;

            if (e.Map != MapManager.Selected.Value)
                return;

            Image = e.Texture;
            BrightnessSprite.ClearAnimations();
            BrightnessSprite.FadeTo(0.80f, Easing.Linear, 250);
        }
    }
}