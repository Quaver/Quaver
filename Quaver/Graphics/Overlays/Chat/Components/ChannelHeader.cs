using System.Drawing;
using Quaver.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Graphics.Overlays.Chat.Components
{
    public class ChannelHeader : Sprite
    {
        /// <summary>
        ///     Reference to the parent chat overlay.
        /// </summary>
        private ChatOverlay Overlay { get; }

        /// <summary>
        ///     The text that displays "Channels"
        /// </summary>
        private SpriteText TextChannels { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        public ChannelHeader(ChatOverlay overlay)
        {
            Overlay = overlay;

            Parent = overlay.ChannelContainer;
            Size = overlay.ChannelHeaderContainner.Size;
            Tint = Colors.DarkGray;
            Alpha = 0.85f;

            CreateChannelsText();
        }

        /// <summary>
        ///     Creates the text that says channels.
        /// </summary>
        private void CreateChannelsText() => TextChannels = new SpriteText(Fonts.Exo2Regular24, "Chat Channels", 0.60f)
        {
            Parent = this,
            Alignment = Alignment.MidCenter
        };
    }
}