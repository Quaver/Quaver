using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbyFilterPanelBanner : SpriteMaskContainer
    {
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
        /// <param name="size"></param>
        public MultiplayerLobbyFilterPanelBanner(ScalableVector2 size)
        {
            Size = size;

            CreateBackgroundSprite();
            CreateBrightnessSprite();
            CreateGradientSprite();
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
    }
}