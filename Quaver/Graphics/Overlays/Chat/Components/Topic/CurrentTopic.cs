using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Graphics.Overlays.Chat.Components.Topic
{
    public class CurrentTopic : Sprite
    {
        /// <summary>
        ///     Reference to the parent chat overlay.
        /// </summary>
        private ChatOverlay Overlay { get; }

        /// <summary>
        ///     The name of the channel.
        /// </summary>
        public SpriteText ChannelName { get; }

        /// <summary>
        ///     The description of the channel.
        /// </summary>
        public SpriteText ChannelDescription { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="overlay"></param>
        public CurrentTopic(ChatOverlay overlay)
        {
            Overlay = overlay;
            Parent = overlay.CurrentTopicContainer;
            Size = overlay.CurrentTopicContainer.Size;

            Tint = Colors.DarkGray;
            Alpha = 0.85f;

            ChannelName = new SpriteText(Fonts.Exo2BoldItalic24, "", 0.60f)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = -10
            };

            ChannelDescription = new SpriteText(Fonts.Exo2Italic24, "", 0.45f)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = 10
            };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void UpdateTopicText(ChatChannel channel)
        {
            ChannelName.Text = channel.Name;
            ChannelName.X = ChannelName.MeasureString().X / 2f + 15;

            ChannelDescription.Text = channel.Description;
            ChannelDescription.X = ChannelDescription.MeasureString().X / 2f + 15;
        }
    }
}