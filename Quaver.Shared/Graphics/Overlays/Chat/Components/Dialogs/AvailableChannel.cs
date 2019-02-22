/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components.Dialogs
{
    public class AvailableChannel : Sprite
    {
        /// <summary>
        ///     The channel this is for.
        /// </summary>
        private ChatChannel Channel { get; }

        /// <summary>
        ///     Reference to the parent dialog.
        /// </summary>
        private JoinChannelDialog Dialog { get; }

        /// <summary>
        ///     The height of the channel.
        /// </summary>
        public static int HEIGHT { get; } = 55;

        /// <summary>
        ///     The button to join/leave the channel.
        /// </summary>
        private BorderedTextButton JoinLeaveButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="dialog"></param>
        public AvailableChannel(ChatChannel chan, JoinChannelDialog dialog)
        {
            Channel = chan;
            Dialog = dialog;
            Tint = Color.Black;
            Alpha = 0.45f;

            Size = new ScalableVector2(dialog.ChannelContainer.Width, HEIGHT);

            var channelName = new SpriteText(Fonts.Exo2BoldItalic, chan.Name, 24)
            {
                Parent = this,
                X = 60,
                Y = 5
            };

            channelName.Size = new ScalableVector2(channelName.Width * 0.55f, channelName.Height * 0.55f);

            var description = new SpriteText(Fonts.Exo2Medium, chan.Description, 24)
            {
                Parent = this,
                X =  channelName.X,
                Y = channelName.Y + channelName.Height
            };

            description.Size = new ScalableVector2(description.Width * 0.50f, description.Height * 0.50f);
            CreateJoinLeaveButton();

            OnlineManager.Client.OnJoinedChatChannel += OnJoinedChatChannel;
            OnlineManager.Client.OnLeftChatChannel += OnLeftChatChannel;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                    ? Color.White
                    : Color.Black, gameTime.ElapsedGameTime.TotalMilliseconds, 30);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnlineManager.Client.OnJoinedChatChannel -= OnJoinedChatChannel;
            OnlineManager.Client.OnLeftChatChannel -= OnLeftChatChannel;
            base.Destroy();
        }

        /// <summary>
        ///     Creates the button to join/leave the chat channel.
        /// </summary>
        private void CreateJoinLeaveButton()
        {
            string text;
            Color color;

            if (ChatManager.JoinedChatChannels.Any(x => x.Name == Channel.Name))
            {
                text = "Leave";
                color = Color.Crimson;
            }
            else
            {
                text = "Join";
                color = Colors.MainAccent;
            }

            JoinLeaveButton = new BorderedTextButton(text, color, (o, e) =>
            {
                lock (ChatManager.JoinedChatChannels)
                {
                    // User needs to leave.
                    if (ChatManager.JoinedChatChannels.Any(x => x.Name == Channel.Name))
                        OnlineManager.Client?.LeaveChatChannel(Channel);
                    // User wants to join
                    else
                        OnlineManager.Client?.JoinChatChannel(Channel.Name);

                    JoinLeaveButton.OriginalColor = Color.White;
                    JoinLeaveButton.Text.Text = "Please Wait...";
                    JoinLeaveButton.IsClickable = false;
                }
            })
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -65,
                Text =
                {
                    FontSize = 13,
                    UsePreviousSpriteBatchOptions = true
                },
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Called when joining a chat channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJoinedChatChannel(object sender, JoinedChatChannelEventArgs e)
        {
            if (e.Channel != Channel.Name)
                return;

            JoinLeaveButton.OriginalColor = Color.Crimson;
            JoinLeaveButton.Text.Text = "Leave";
            JoinLeaveButton.IsClickable = true;
        }

        /// <summary>
        ///     Called when we've left a chat channel successfully.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeftChatChannel(object sender, LeftChatChannelEventArgs e)
        {
            if (e.ChannelName != Channel.Name)
                return;

            JoinLeaveButton.OriginalColor = Colors.MainAccent;
            JoinLeaveButton.Text.Text = "Join";
            JoinLeaveButton.IsClickable = true;
        }
    }
}
