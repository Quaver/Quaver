using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Channels;
using Quaver.Shared.Online.Chat;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI
{
    public class MenuFooterButtonGameChat : ButtonText
    {
        /// <summary>
        /// </summary>
        private Sprite UnreadNotification { get; }

        /// <summary>
        /// </summary>
        private ChatChannel MultiplayerChannel { get; }

        /// <summary>
        /// </summary>
        private ChatChannelListButton ChannelButton { get; }

        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="onClicked"></param>
        public MenuFooterButtonGameChat(BitmapFont font, string text, int fontSize, EventHandler onClicked = null) : base(font, text, fontSize, onClicked)
        {
            UnreadNotification = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(10, 10),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_circle),
                Tint = Color.Yellow,
                Position = new ScalableVector2(17, 0),
            };

            MultiplayerChannel = ChatManager.JoinedChatChannels.Find(x => x.Name.StartsWith("#multiplayer"));
            ChannelButton = ChatManager.Dialog.ChatChannelList.Buttons.Find(x => x.Channel == MultiplayerChannel);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (MultiplayerChannel != null && ChannelButton.IsUnread)
            {
                UnreadNotification.Visible = true;

                if (UnreadNotification.Animations.Count == 0)
                {
                    UnreadNotification
                        .FadeTo(0.3f, Easing.Linear, 600)
                        .Wait();

                    UnreadNotification.FadeTo(1, Easing.Linear, 600);
                }
                else
                {
                    if (UnreadNotification.Animations.First().Properties != AnimationProperty.Wait)
                    {
                        Text.Alpha = UnreadNotification.Alpha;
                    }
                }
            }
            else
            {
                UnreadNotification.Visible = false;
                Text.Alpha = 1;
            }

            base.Update(gameTime);
        }
    }
}