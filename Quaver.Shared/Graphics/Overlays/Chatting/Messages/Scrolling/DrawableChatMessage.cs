using System;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client.Structures;
using Quaver.Server.Client.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Scrolling;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Messages.Scrolling
{
    public sealed class DrawableChatMessage : PoolableSprite<ChatMessage>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = 44;

        /// <summary>
        ///     The time the message was displayed
        /// </summary>
        private SpriteTextPlus Time { get; set; }

        /// <summary>
        ///     An icon to represent the user
        /// </summary>
        private Sprite Icon { get; set; }

        /// <summary>
        ///     The clan tag of the user
        /// </summary>
        private ClanTag Clan { get; set; }

        /// <summary>
        ///     The username of the user
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <summary>
        /// </summary>
        private DrawableChatMessageUsernameButton UsernameButton { get; set; }

        /// <summary>
        ///     The message the user sent
        /// </summary>
        private SpriteTextPlus Message { get; set; }

        /// <summary>
        ///     The amount of padding on the x axis to use
        /// </summary>
        private const int PADDING_X = 16;

        private bool IsScrollVisible { get; set; } = true;

        private bool IconShouldBeVisible { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableChatMessage(PoolableScrollContainer<ChatMessage> container, ChatMessage item, int index) : base(container, item, index)
        {
            Tint = Index % 2 == 0 ? Colors.DarkGray : Colors.BlueishDarkGray;
            Size = new ScalableVector2(container.Width, HEIGHT);
            Alpha = 0;

            CreateTime();
            CreateIcon();
            CreateClan();
            CreateUsername();
            CreateUsernameButton();
            CreateMessage();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!IsScrollVisible)
                return;

            var color = Colors.GetUserChatColor(Item.Sender.OnlineUser.UserGroups);
            Username.Tint = UsernameButton.IsHovered ? Darken(color) : color;
            Icon.Tint = Username.Tint;

            Clan.Tint = UsernameButton.IsHovered ? Darken(Clan.BaseColor) : Clan.BaseColor;

            var container = (ChatMessageScrollContainer) Container;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(ChatMessage item, int index)
        {
            Item = item;
            Index = index;

            var dateTime = DateTimeOffset.FromUnixTimeMilliseconds((long) item.Time).ToLocalTime();

            Time.Text = dateTime.Humanize(culture: LocalizationManager.CurrentCulture);
            var icon = GetIcon(Item.Sender.OnlineUser.UserGroups);
            var labelStartX = Time.X + Time.Width + PADDING_X / 2f;

            if (icon != null)
            {
                Icon.Image = icon;
                Icon.Tint = Colors.GetUserChatColor(Item.Sender.OnlineUser.UserGroups);
                Icon.X = labelStartX;

                labelStartX = Icon.X + Icon.Width + PADDING_X / 2f;
            }

            IconShouldBeVisible = icon != null;
            Icon.Visible = IsScrollVisible && IconShouldBeVisible;

            Clan.UpdateFromClan(GetClanTag(), GetClanAccentColor(), Colors.GetUserChatColor(Item.Sender.OnlineUser.UserGroups));
            Clan.X = labelStartX;

            Username.X = Clan.HasClan ? Clan.X + Clan.Width + PADDING_X / 4f : labelStartX;
            Username.Text = $"{Item.SenderName}:";
            Username.Tint = Colors.GetUserChatColor(Item.Sender.OnlineUser.UserGroups);

            var clickableStartX = IconShouldBeVisible ? Icon.X : Clan.HasClan ? Clan.X : Username.X;
            var clickableWidth = Username.X + Username.Width - clickableStartX;

            Message.X = clickableStartX + clickableWidth + PADDING_X / 2f;
            Message.Text = Item.Message;

            Message.MaxWidth = Container.Width - Message.X - PADDING_X;

            Time.Alignment = Alignment.MidLeft;
            Time.Y = 0;

            Icon.Alignment = Time.Alignment;
            Icon.Y = 0;

            Clan.Alignment = Time.Alignment;
            Clan.Y = 0;

            Username.Alignment = Time.Alignment;
            Username.Y = 0;

            // Message spans across multiple lines, so make sure the username is moved upwards
            if (Message.Children.Count > 1)
            {
                Time.Alignment = Alignment.TopLeft;
                Time.Y = PADDING_X / 2f;

                Icon.Alignment = Time.Alignment;
                Icon.Y = Time.Y + 2;

                Clan.Alignment = Time.Alignment;
                Clan.Y = Time.Y;

                Username.Alignment = Time.Alignment;
                Username.Y = Time.Y;
            }

            // Make sure the button is properly aligned with the sender's username
            UsernameButton.Alignment = Username.Alignment;
            UsernameButton.Position = new ScalableVector2(clickableStartX, Username.Y);
            UsernameButton.Size = new ScalableVector2(clickableWidth, Username.Height);

            Height = Message.Height + 16;
            ApplyScrollVisibility();
        }

        /// <summary>
        ///     Creates <see cref="Time"/>
        /// </summary>
        private void CreateTime()
        {
            Time = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), "", 18)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = PADDING_X,
                Tint = ColorHelper.HexToColor("#a8a8a8"),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="Icon"/>
        /// </summary>
        private void CreateIcon()
        {
            Icon = new Sprite()
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(16, 16)
            };
        }

        /// <summary>
        ///     Creates <see cref="Clan"/>
        /// </summary>
        private void CreateClan()
        {
            Clan = new ClanTag(Time.FontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="Username"/>
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "", Time.FontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUsernameButton()
        {
            UsernameButton = new DrawableChatMessageUsernameButton(UserInterface.BlankBox, Container)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Alpha = 0
            };

            var container = (ChatMessageScrollContainer) Container;

            UsernameButton.Clicked += (sender, args) => container.ActivateRightClickOptions(new DrawableOnlineUserRightClickOptions(Item.Sender));
            UsernameButton.RightClicked += (sender, args) => container.ActivateRightClickOptions(new DrawableOnlineUserRightClickOptions(Item.Sender));
        }

        /// <summary>
        ///     Creates <see cref="Message"/>
        /// </summary>
        private void CreateMessage()
        {
            Message = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), "", Time.FontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Gets a chat badge icon based on user groups.
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static Texture2D GetIcon(UserGroups groups)
        {
            if (groups.HasFlag(UserGroups.Swan))
                return FontAwesome.Get(FontAwesomeIcon.fa_code);

            if (groups.HasFlag(UserGroups.Developer))
                return FontAwesome.Get(FontAwesomeIcon.fa_code);

            // Bot
            if (groups.HasFlag(UserGroups.Bot))
                return FontAwesome.Get(FontAwesomeIcon.fa_open_wrench_tool_silhouette);

            // Admin
            if (groups.HasFlag(UserGroups.Admin))
                return FontAwesome.Get(FontAwesomeIcon.fa_legal_hammer);

            if (groups.HasFlag(UserGroups.Moderator))
                return FontAwesome.Get(FontAwesomeIcon.fa_ban_circle_symbol);

            if (groups.HasFlag(UserGroups.RankingSupervisor))
                return FontAwesome.Get(FontAwesomeIcon.fa_music_note_black_symbol);

            if (groups.HasFlag(UserGroups.Contributor))
                return FontAwesome.Get(FontAwesomeIcon.fa_light_bulb);

            if (groups.HasFlag(UserGroups.Donator))
                return FontAwesome.Get(FontAwesomeIcon.fa_heart_shape_silhouette);

            return null;
        }

        public void SetScrollVisibility(bool visible)
        {
            IsScrollVisible = visible;
            ApplyScrollVisibility();
        }

        private void ApplyScrollVisibility()
        {
            Visible = IsScrollVisible;
            Time.Visible = IsScrollVisible;
            Icon.Visible = IsScrollVisible && IconShouldBeVisible;
            Clan.Visible = IsScrollVisible && Clan.HasClan;
            Username.Visible = IsScrollVisible;
            UsernameButton.Visible = IsScrollVisible;
            UsernameButton.IsClickable = IsScrollVisible;
            Message.Visible = IsScrollVisible;
        }

        /// <summary>
        ///     Gets the sender's clan tag from the chat payload, falling back to the sender's online user info.
        /// </summary>
        private string? GetClanTag() => !string.IsNullOrEmpty(Item.SenderClanTag)
            ? Item.SenderClanTag
            : Item.Sender?.OnlineUser?.ClanTag;

        /// <summary>
        ///     Gets the sender's clan accent color from the chat payload, falling back to the sender's online user info.
        /// </summary>
        private string? GetClanAccentColor() => !string.IsNullOrEmpty(Item.SenderClanAccentColor)
            ? Item.SenderClanAccentColor
            : Item.Sender?.OnlineUser?.ClanAccentColor;

        /// <summary>
        ///     Darkens a color for hover state.
        /// </summary>
        private static Color Darken(Color color) => new Color(color.R / 2, color.G / 2, color.B / 2);
    }
}
