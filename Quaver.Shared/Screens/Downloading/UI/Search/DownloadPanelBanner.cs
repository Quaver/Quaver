using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Search
{
    public class DownloadPanelBanner : SpriteMaskContainer
    {
        /// <summary>
        /// </summary>
        private Bindable<DownloadableMapset> SelectedMapset { get; }

        /// <summary>
        ///     The current map's background
        /// </summary>
        public BackgroundImage Background { get; private set; }

        /// <summary>
        ///     A gradient overlay layer of the banner
        /// </summary>
        private Sprite Gradient { get; set; }

        /// <summary>
        ///     Controls the overall brightness of the banner
        /// </summary>
        private Sprite Brightness { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Subheader { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="selectedMapset"></param>
        public DownloadPanelBanner(ScalableVector2 size, Bindable<DownloadableMapset> selectedMapset)
        {
            Size = size;
            SelectedMapset = selectedMapset;

            CreateBackgroundSprite();
            CreateBrightnessSprite();
            CreateGradientSprite();
            CreateHeaderText();

            SelectedMapset.ValueChanged += OnSelectedMapsetChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SelectedMapset.ValueChanged -= OnSelectedMapsetChanged;
            base.Destroy();
        }

        /// <summary>
        ///     Creates the background that is masked
        /// </summary>
        private void CreateBackgroundSprite()
        {
            Background = new BackgroundImage(UserInterface.MenuBackgroundNormal, 0, false)
            {
                BrightnessSprite =
                {
                    Visible = false,
                },
                Alignment = Alignment.MidCenter,
                Y = -460
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
        /// </summary>
        private void CreateHeaderText()
        {
            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Download Maps", 26)
            {
                Parent = this,
                X = 24,
                Y = 18
            };

            Subheader = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Find new songs to play from the community", 22)
            {
                Parent = this,
                X = Header.X,
                Y = -Header.Y,
                Alignment = Alignment.BotLeft,
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedMapsetChanged(object sender, BindableValueChangedEventArgs<DownloadableMapset> e)
        {
        }
    }
}