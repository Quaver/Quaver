using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Resources;
using Quaver.Graphics.Overlays.Chat.Components.Dialogs;
using Quaver.Screens.Menu.UI.Navigation.User;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
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

        /// <summary>
        ///     The button to join chat channels.
        /// </summary>
        private BorderedTextButton JoinButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        public ChannelHeader(ChatOverlay overlay)
        {
            Overlay = overlay;

            Parent = overlay.ChannelContainer;
            Size = overlay.ChannelHeaderContainner.Size;
            Tint = Color.Black;
            Alpha = 0.85f;

            CreateChannelsText();
            CreateJoinButton();
        }

        /// <summary>
        ///     Creates the text that says channels.
        /// </summary>
        private void CreateChannelsText()
        {
            TextChannels = new SpriteText(BitmapFonts.Exo2Medium, "Chat Channels", 13)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 15
            };
        }

        /// <summary>
        ///     Creates the button to join chat channels.
        /// </summary>
        private void CreateJoinButton() => JoinButton = new BorderedTextButton("Join", Colors.MainAccent,
            (o, e) => Overlay.OpenJoinChannelDialog())
        {
            Parent = this,
            Alignment = Alignment.MidRight,
            X = -TextChannels.X,
            Y = -2,
            Size = new ScalableVector2(75, 30),
            Text =
            {
                FontSize = 13
            }
        };
    }
}