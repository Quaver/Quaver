/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Online.Chat;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components.Dialogs
{
    public class JoinChannelDialog : DialogScreen
    {
        /// <summary>
        ///     Reference to the chat overlay dialog.
        /// </summary>
        private ChatOverlay Overlay { get; }

        /// <summary>
        ///     The actual content of the interface
        /// </summary>
        public Sprite InterfaceContainer { get; private set; }

        /// <summary>
        ///     The container for the header of the interface.
        /// </summary>
        private Sprite HeaderContainer { get; set; }

        /// <summary>
        ///     The scroll container which houses all of the chat channels.
        /// </summary>
        public ScrollContainer ChannelContainer { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public JoinChannelDialog(ChatOverlay overlay) : base(0)
        {
            Overlay = overlay;
            CreateContent();

            Clicked += (sender, args) =>
            {
                if (!GraphicsHelper.RectangleContains(InterfaceContainer.ScreenRectangle,
                    MouseManager.CurrentState.Position))
                {
                    ChatManager.Dialog.CloseJoinChannelDialog();
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateInterfaceContainer();
            CreateHeaderContainer();
            CreateChannelContainer();
            AddChannels();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Overlay.CloseJoinChannelDialog();
        }

        /// <summary>
        ///     Creates the container for the entire dialog.
        /// </summary>
        private void CreateInterfaceContainer() => InterfaceContainer = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.BotLeft,
            SetChildrenAlpha = true,
            Size = new ScalableVector2(Width, 350),
            Tint = new Color(63, 68, 91),
            Y = 350,
            Alpha = 0,
            Animations =
            {
                new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 400),
                new Animation(AnimationProperty.Y, Easing.OutQuint, 400, 0, 800)
            },
        };

        /// <summary>
        ///     Creates the header container for the interface.
        /// </summary>
        private void CreateHeaderContainer()
        {
            HeaderContainer = new Sprite()
            {
                Parent = InterfaceContainer,
                Size = new ScalableVector2(Width, 75),
                Tint = Colors.DarkGray,
            };

            var line = new Sprite()
            {
                Parent = HeaderContainer,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(HeaderContainer.Width, 2),
                Tint = Colors.MainAccent
            };

            var icon = new Sprite()
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidLeft,
                X = 25,
                Size = new ScalableVector2(HeaderContainer.Height * 0.50f, HeaderContainer.Height * 0.50f),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_group_profile_users),
            };

            var chatChannels = new SpriteText(Fonts.Exo2SemiBold, "Join Chat Channels", 14)
            {
                Parent = icon,
                Y = -3,
                X = icon.Width + 15,
            };

            var description = new SpriteText(Fonts.Exo2SemiBold, "Channels are divided into individual chat topics. Join one! What are you waiting for?",
                13)
            {
                Parent = icon,
                Y = chatChannels.Y + chatChannels.Height - 2,
                X = icon.Width + 15,
            };
        }

        /// <summary>
        ///     Creates the ScrollContainer that displays the chat channels.
        /// </summary>
        private void CreateChannelContainer()
        {
            var size = new ScalableVector2(Width, InterfaceContainer.Height - HeaderContainer.Height);
            ChannelContainer = new ScrollContainer(size, size)
            {
                Parent = InterfaceContainer,
                Y = HeaderContainer.Height,
                Alpha = 0.50f,
                Tint = Color.Black,
                InputEnabled = true,
                ScrollSpeed = 150,
                EasingType = Easing.OutQuint,
                TimeToCompleteScroll = 1500,
                Scrollbar =
                {
                    Tint = Color.White,
                    Width = 3,
                }
            };
        }

        /// <summary>
        ///    Adds all the available channels to the dialog container.
        /// </summary>
        private void AddChannels()
        {
            lock (ChatManager.AvailableChatChannels)
            {
                for (var i = 0; i < ChatManager.AvailableChatChannels.Count; i++)
                {
                    var chan = ChatManager.AvailableChatChannels[i];

                    var availableChannel = new AvailableChannel(chan, this)
                    {
                        Y = AvailableChannel.HEIGHT * i
                    };

                    availableChannel.Y += (i + 1) * 10;

                    ChannelContainer.AddContainedDrawable(availableChannel);
                }

                var totalHeight = ChatManager.AvailableChatChannels.Count * AvailableChannel.HEIGHT + ((ChatManager.AvailableChatChannels.Count + 1) * 10);

                if (totalHeight > ChannelContainer.Height)
                    ChannelContainer.ContentContainer.Height = totalHeight - 1;
            }
        }
    }
}
