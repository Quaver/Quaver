using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Messages.Textbox
{
    public class OnlineChatTextbox : Wobble.Graphics.UI.Form.Textbox
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChannel { get; }

        /// <summary>
        /// </summary>
        /// <param name="activeChatChannel"></param>
        /// <param name="size"></param>
        public OnlineChatTextbox(Bindable<ChatChannel> activeChatChannel, ScalableVector2 size)
            : base(size, FontManager.GetWobbleFont(Fonts.LatoBlack), 20, "", "Send a message...")
        {
            ActiveChannel = activeChatChannel;

            Image = UserInterface.SearchBox;
            Tint = ColorHelper.HexToColor("#181818");

            OnSubmit += msg =>
            {
                if (ActiveChannel.Value == null)
                    return;

                var user = OnlineManager.Self;

                // ONLY USED FOR TESTING
                if (user == null)
                {
                    user = new User()
                    {
                        OnlineUser = new OnlineUser()
                        {
                            Id = -1,
                            CountryFlag = "US",
                            SteamId = 0,
                            UserGroups = UserGroups.Admin,
                            Username = "God"
                        }
                    };
                }

                foreach (var word in msg.Split(" "))
                {
                    if (!EmojiHelper.Emojis.ContainsKey(word))
                        continue;

                    msg = msg.Replace(word, char.ConvertFromUtf32(EmojiHelper.Emojis[word]));
                }

                var message = new ChatMessage(user.OnlineUser.Id, user.OnlineUser.Username, ActiveChannel.Value.Name,
                    msg, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                {
                    Sender = user,
                    IsFromSelf = true
                };

                if (message.Message.StartsWith("/"))
                {
                    QuaverBot.HandleClientSideCommands(message);
                    return;
                }

                ActiveChannel.Value?.QueueMessage(message);
                OnlineManager.Client?.SendMessage(ActiveChannel.Value.Name, message.Message);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (OnlineChat.Instance != null)
            {
                if (OnlineChat.Instance.IsOpen && OnlineChat.Instance.IsHovered())
                {
                    InputEnabled = true;
                    Focused = true;
                }
                else if (!OnlineChat.Instance.IsOpen)
                {
                    InputEnabled = false;
                    Focused = false;
                }
            }

            base.Update(gameTime);
        }
    }
}