using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Scrolling
{
    public sealed class DrawableOnlineUser : PoolableSprite<User>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 61;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Flag { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Status { get; set; }

        /// <summary>
        /// </summary>
        private Sprite OnlineStatusIcon { get; set; }

        /// <summary>
        /// </summary>
        private OnlineUser OnlineUser => Item.OnlineUser;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableOnlineUser(PoolableScrollContainer<User> container, User item, int index) : base(container, item, index)
        {
            Size = new ScalableVector2(container.Width, HEIGHT);
            Alpha = 0;

            CreateButton();
            CreateAvatar();
            CreateFlag();
            CreateUsername();
            CreateStatus();
            CreateOnlineStatusIcon();

            SteamManager.SteamUserAvatarLoaded += OnAvatarLoaded;

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnUserInfoReceived += OnUserInfoReceived;
                OnlineManager.Client.OnUserStatusReceived += OnUserStatusReceived;

                // If we're logged in, we already have our user state, so we can update content
                // immediately
                if (Item.OnlineUser.Id == OnlineManager.Self.OnlineUser.Id)
                    UpdateContent(OnlineManager.Self, Index);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (Button != null)
            {
                Button.Alpha = Button.IsHovered ? 0.45f : 0;

                var container = (OnlineUserContainer) Container;

                Button.Depth = container.FilterDropdown.Dropdown.Opened ||
                               container.ActiveRightClickOptions!= null && container.ActiveRightClickOptions.Opened ? 1 : 0;

                var game = (QuaverGame) GameBase.Game;

                if (Container != null)
                    Button.IsClickable = game.OnlineHub.SelectedSection == game.OnlineHub.Sections[OnlineHubSectionType.OnlineUsers];
            }

            // The button is no longer in range of the container, so uncontain it.
            if (Container != null && RectangleF.Intersection(ScreenRectangle, Container.ScreenRectangle).IsEmpty)
                Container.RemoveContainedDrawable(this);

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SteamManager.SteamUserAvatarLoaded -= OnAvatarLoaded;

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnUserInfoReceived -= OnUserInfoReceived;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(User item, int index)
        {
            Item = item;

            // Set self user since it's pre-made for us
            if (Item.OnlineUser.Id == OnlineManager.Self.OnlineUser.Id)
                Item = OnlineManager.Self;

            Index = index;

            Tint = index % 2 == 0 ? Colors.BlueishDarkGray : Colors.DarkGray;

            ScheduleUpdate(() =>
            {
                var steamId = (ulong) OnlineUser.SteamId;

                Avatar.ClearAnimations();

                if (SteamManager.UserAvatars.ContainsKey(steamId))
                {
                    Avatar.Image = SteamManager.UserAvatars[steamId];
                    Avatar.Alpha = 1;
                }
                else
                {
                    Avatar.Alpha = 0;
                    SteamManager.SendAvatarRetrievalRequest(steamId);
                }

                // Empty username is a valid way to check if we have user information
                if (!Item.HasUserInfo)
                {
                    Username.Text = $"Loading (#{OnlineUser.Id})...";
                    Username.Tint = Color.White;
                    Flag.Image = Flags.Get("XX");
                    Status.Text = "Idle";
                }
                else
                {
                    Username.Text = OnlineUser.Username;
                    Username.Tint = Colors.GetUserChatColor(OnlineUser.UserGroups);
                    Flag.Image = Flags.Get(OnlineUser.CountryFlag);
                    UpdateUserStatus();
                }

                Avatar.Border.Tint = Username.Tint;
            });
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new DrawableOnlineUserButton(UserInterface.BlankBox, Container)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };

            var container = (OnlineUserContainer) Container;

            Button.Clicked += (sender, args) => container.ActivateRightClickOptions(new DrawableOnlineUserRightClickOptions(Item));
            Button.RightClicked += (sender, args) => container.ActivateRightClickOptions(new DrawableOnlineUserRightClickOptions(Item));
        }

        /// <summary>
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(40, 40),
                X = 16,
                Image = UserInterface.UnknownAvatar,
                UsePreviousSpriteBatchOptions = true,
                SetChildrenAlpha = true,
            };

            Avatar.AddBorder(Color.White, 2);
            Avatar.Alpha = 0;
        }

        /// <summary>
        /// </summary>
        private void CreateFlag()
        {
            Flag = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(25, 25),
                X = Avatar.X + Avatar.Width + 14,
                Y = 8,
                Image = Flags.Get("XX"),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Loading...", 22)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = Flag.X + Flag.Width + 8,
                Y = Flag.Y,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateStatus()
        {
            Status = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Idle", 20)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Tint = ColorHelper.HexToColor("#a6a6a6"),
                X = Flag.X,
                Y = -Flag.Y,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateOnlineStatusIcon()
        {
            OnlineStatusIcon = new Sprite
            {
                Parent = Avatar,
                Alignment = Alignment.BotRight,
                Size = new ScalableVector2(18, 18),
                UsePreviousSpriteBatchOptions = true,
                Image = UserInterface.HubOnlineIcon
            };

            OnlineStatusIcon.Position = new ScalableVector2(OnlineStatusIcon.Width / 4f,OnlineStatusIcon.Height / 4f);
        }
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            if (e.SteamId != (ulong) OnlineUser.SteamId)
                return;

            Avatar.ClearAnimations();
            Avatar.Image = e.Texture;
            Avatar.FadeTo(1, Easing.Linear, 200);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserInfoReceived(object sender, UserInfoEventArgs e)
        {
            var user = e.Users.Find(x => x.Id == OnlineUser.Id);

            if (user == null)
                return;

            UpdateContent(Item, Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserStatusReceived(object sender, UserStatusEventArgs e)
        {
            if (!e.Statuses.ContainsKey(OnlineUser.Id))
                return;

            UpdateContent(Item, Index);
        }

        /// <summary>
        /// </summary>
        private void UpdateUserStatus()
        {
            var status = Item.CurrentStatus;

            switch (status.Status)
            {
                case ClientStatus.InMenus:
                    Status.Text = $"Idle";
                    break;
                case ClientStatus.Selecting:
                    Status.Text = "Selecting a Song";
                    break;
                case ClientStatus.Playing:
                    Status.Text = $"Playing {status.Content}";
                    break;
                case ClientStatus.Paused:
                    Status.Text = $"Paused in Gameplay";
                    break;
                case ClientStatus.Watching:
                    Status.Text = $"Watching {status.Content}";
                    break;
                case ClientStatus.Editing:
                    Status.Text = $"Editing {status.Content}";
                    break;
                case ClientStatus.InLobby:
                    Status.Text = $"Finding a Multiplayer Game";
                    break;
                case ClientStatus.Multiplayer:
                    Status.Text = $"Playing Multiplayer";
                    break;
                case ClientStatus.Listening:
                    Status.Text = $"Listening to {status.Content}";
                    break;
                default:
                    Status.Text = "Idle";
                    break;
            }

            Status.TruncateWithEllipsis(350);
        }
    }
}