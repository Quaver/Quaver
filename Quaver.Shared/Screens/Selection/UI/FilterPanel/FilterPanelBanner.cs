using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel
{
    public class FilterPanelBanner : SpriteMaskContainer
    {
        /// <summary>
        ///     The current map's background
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        ///     A gradient overlay layer of the banner
        /// </summary>
        private Sprite Gradient { get; set; }

        /// <summary>
        ///     Controls the overall brightness of the banner
        /// </summary>
        private Sprite Brightness { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="panel"></param>
        public FilterPanelBanner(SelectFilterPanel panel)
        {
            Size = new ScalableVector2(960, panel.Height);

            CreateBackgroundSprite();
            CreateBrightnessSprite();
            CreateGradientSprite();

            BackgroundHelper.Loaded += OnBackgroundLoaded;
            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundHelper.Loaded -= OnBackgroundLoaded;

            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Creates the background that is masked
        /// </summary>
        private void CreateBackgroundSprite()
        {
            Background = new BackgroundImage(BackgroundHelper.RawTexture ?? UserInterface.MenuBackgroundNormal, 0, false)
            {
                BrightnessSprite =
                {
                    Visible = false,
                },
                Alignment = Alignment.MidCenter
            };

            AddContainedSprite(Background);
        }

        /// <summary>
        ///     Creates the sprite that controls the brightness of the banner
        /// </summary>
        private void CreateGradientSprite()
        {
            Gradient = new Sprite
            {
                Parent = this,
                Size = Size,
                Image = UserInterface.FilterPanelGradient
            };
        }

        /// <summary>
        ///     Creates the sprite that controls brightness
        /// </summary>
        private void CreateBrightnessSprite()
        {
            Brightness = new Sprite
            {
                Parent = this,
                Size = Size,
                Tint = ColorHelper.HexToColor("#242424"),
                Alpha = 0.50f
            };
        }

        /// <summary>
        ///     Fades the banner to black
        /// </summary>
        public void FadeToBlack()
        {
            Brightness.ClearAnimations();
            Brightness.FadeTo(1, Easing.OutQuint, 700);
        }

        /// <summary>
        ///     Fades the banner back in
        /// </summary>
        public void FadeIn()
        {
            Brightness.ClearAnimations();
            Brightness.FadeTo(0.5f, Easing.OutQuint, 700);
        }

        /// <summary>
        ///     Called when the background of a new map has successfully loaded.
        ///         * This will switch the background and fade the container back in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            if (MapManager.Selected?.Value != e.Map)
                return;

            Background.Image = e.Texture;
            FadeIn();
        }

        /// <summary>
        ///     Called when the selected map has changed.
        ///         * This will fade the background completely
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => FadeToBlack();
    }
}