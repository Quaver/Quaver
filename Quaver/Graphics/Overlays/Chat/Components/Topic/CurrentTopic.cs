using System;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Logging;
using Quaver.Online;
using Quaver.Online.Chat;
using Quaver.Scheduling;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;

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

        /// <summary>
        ///     The button to close the chat channel.
        /// </summary>
        public TextButton CloseChannelButton { get; }

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

            CloseChannelButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "Close Channel", 0.55f)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(150, 40),
                X = -15,
                Tint = Color.Red
            };

            CloseChannelButton.Clicked += (sender, args) => CloseActiveChatChannel();
        }

        /// <summary>
        ///     Updates the topic text & close channel button with a new channel.
        /// </summary>
        /// <param name="channel"></param>
        public void UpdateTopicText(ChatChannel channel)
        {
            ChannelName.Text = channel.Name;
            ChannelName.X = ChannelName.MeasureString().X / 2f + 15;

            ChannelDescription.Text = channel.Description;
            ChannelDescription.X = ChannelDescription.MeasureString().X / 2f + 15;

            // Only display the close channel button on private channels.
            CloseChannelButton.Visible = channel.IsPrivate;
            CloseChannelButton.IsClickable = channel.IsPrivate;
        }

        /// <summary>
        ///     Closes the chat channel.
        /// </summary>
        private void CloseActiveChatChannel()
        {
            // Only allow private channels to be close for now.
            if (!Overlay.ActiveChannel.IsPrivate)
                return;

            var channelButton = Overlay.ChatChannelList.SelectedButton;

            var tfX = new Transformation(TransformationProperty.X, Easing.Linear, channelButton.X, -(channelButton.Width + 5), 100);
            channelButton.Transformations.Add(tfX);

            // Check to see if there is another button before this one.
            var buttonIndex = Overlay.ChatChannelList.Buttons.FindIndex(x => x == channelButton);

            // If there ends up being a button, then we'll want to go through with the removal process and selecting the
            // previous chat channel.
            if (buttonIndex != -1 && buttonIndex - 1 >= 0)
            {
                // Remove the button from the list of chat channel buttons.
                Overlay.ChatChannelList.Buttons.Remove(channelButton);

                // Remove the channel from the joined chat channels.
                ChatManager.JoinedChatChannels.Remove(Overlay.ActiveChannel);

                // Remove the chat channel container
                var messageContainer = Overlay.ChannelMessageContainers[Overlay.ActiveChannel];
                Overlay.ChannelMessageContainers.Remove(Overlay.ActiveChannel);

                // Destroy the container instantly.
                messageContainer.Destroy();

                // Make sure the buttons are realigned properly after closing this channel.
                Overlay.ChatChannelList.RealignButtons();

                // Destroy the button after it finishes its transformation.
                Scheduler.RunAfter(() => channelButton.Destroy(), 150);

                var newSelectedButton = Overlay.ChatChannelList.Buttons[buttonIndex - 1];
                newSelectedButton.SelectChatChannel();

                Logger.LogInfo($"Closed chat channel: `{channelButton.Channel.Name}`", LogType.Runtime);
            }

            // TODO: Handle this for public chats.
        }
    }
}