using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting
{
    internal sealed class ChatMessageRow : Container
    {
        private const float VerticalPadding = 8;
        private SpriteTextPlus Timestamp { get; }

        private TooltipOptions TimestampTooltipOptions { get; } = new TooltipOptions();

        private IDisposable TimestampTooltipRegistration { get; }

        private FlexContainer SenderContainer { get; }

        private Container SenderBadgeContainer { get; }

        private Sprite SenderBadge { get; }

        private FlexItemOptions SenderBadgeLayoutOptions { get; }

        private SpriteTextPlus SenderClan { get; }

        private SpriteTextPlus Sender { get; }

        private RoundedButton SenderButton { get; }

        private SpriteTextPlus Message { get; }

        private Color SenderColor { get; set; }

        private Color SenderClanColor { get; set; }

        public ChatMessage Item { get; private set; }

        public event Action<User> SenderMenuRequested;

        public ChatMessageRow(ChatMessage item, float width, ScrollContainer viewport)
        {
            Size = new ScalableVector2(width, 40);

            Timestamp = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 18)
            {
                Parent = this,
                Position = new ScalableVector2(10, VerticalPadding),
                Tint = ColorHelper.FromHex("#D0DBED")
            };
            TimestampTooltipRegistration = Timestamp.AddTooltip(TimestampTooltipOptions);

            SenderContainer = new FlexContainer
            {
                Parent = this,
                Position = new ScalableVector2(100, VerticalPadding),
                Size = new ScalableVector2(1, 40 - VerticalPadding * 2),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center
            };

            SenderBadgeContainer = new Container
            {
                Parent = SenderContainer,
                Size = new ScalableVector2(UserGroupAssets.SmallBadgeWidth + 5, SenderContainer.Height),
                Visible = false
            };
            SenderBadgeLayoutOptions = new FlexItemOptions { Basis = 0, Shrink = 0 };
            SenderContainer.SetItemOptions(SenderBadgeContainer, SenderBadgeLayoutOptions);

            SenderBadge = new Sprite
            {
                Parent = SenderBadgeContainer,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(UserGroupAssets.SmallBadgeWidth, UserGroupAssets.BadgeHeight),
                Visible = false,
                UsePreviousSpriteBatchOptions = true
            };

            SenderClan = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "", 18)
            {
                Parent = SenderContainer,
                UsePreviousSpriteBatchOptions = true
            };
            SenderContainer.SetItemOptions(SenderClan, new FlexItemOptions { Shrink = 0 });

            Sender = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 18)
            {
                Parent = SenderContainer,
                Alignment = Alignment.MidLeft,
                Tint = ColorHelper.FromHex("#FFFFFF"),
                UsePreviousSpriteBatchOptions = true
            };
            SenderContainer.SetItemOptions(Sender, new FlexItemOptions { Shrink = 0 });

            Message = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), string.Empty, 18)
            {
                Parent = this,
                Tint = ColorHelper.FromHex("#9FA8B6")
            };

            // This button is invisible and is only here to apply the hover effect on the sender components
            SenderButton = new ViewportRoundedButton(viewport)
            {
                Parent = this,
                Position = SenderContainer.Position,
                Size = SenderContainer.Size,
                Tint = Color.Transparent,
                CornerRadius = 0,
                PerformHoverFade = false
            };
            SenderButton.Hovered += (sender, args) => ApplySenderHover(true);
            SenderButton.LeftHover += (sender, args) => ApplySenderHover(false);
            SenderButton.RightClicked += (sender, args) =>
            {
                if (Item?.Sender != null)
                    SenderMenuRequested?.Invoke(Item.Sender);
            };

            UpdateContent(item, width);
        }

        public void UpdateContent(ChatMessage item, float width)
        {
            Item = item;
            Width = width;

            var time = DateTimeOffset.FromUnixTimeMilliseconds((long)item.Time).ToLocalTime();
            Timestamp.Text = $"{time.Hour:00}:{time.Minute:00}:{time.Second:00}";
            TimestampTooltipOptions.Text = time.ToString("dddd dd MMMM yyyy HH:mm", LocalizationManager.CurrentCulture);

            var senderGroups = item.Sender.OnlineUser.UserGroups;
            var badge = GetBadge(senderGroups);
            SenderBadgeContainer.Visible = badge != null;
            SenderBadge.Visible = badge != null;
            SenderBadgeLayoutOptions.Basis = badge == null ? 0 : UserGroupAssets.SmallBadgeWidth + 5;
            if (badge != null)
                SenderBadge.Region = badge.Value;

            var clanTag = !string.IsNullOrEmpty(item.SenderClanTag) ? item.SenderClanTag : item.Sender.OnlineUser.ClanTag;
            var hasClan = !string.IsNullOrEmpty(clanTag);

            SenderClan.Visible = hasClan;
            SenderClan.Text = hasClan ? $"[{clanTag}] " : "";
            var clanColor = !string.IsNullOrEmpty(item.SenderClanAccentColor) ? item.SenderClanAccentColor : item.Sender.OnlineUser.ClanAccentColor;
            SenderColor = Colors.GetUserChatColor(senderGroups);
            SenderClanColor = clanColor != null ? ColorHelper.FromHex(clanColor) : SenderColor;

            Sender.Text = $"{item.SenderName}:";

            SenderContainer.Width = (SenderBadgeLayoutOptions.Basis ?? 0) + SenderClan.Width + Sender.Width;
            SenderContainer.RefreshLayout();
            SenderButton.Position = SenderContainer.Position;
            SenderButton.Size = SenderContainer.Size;
            ApplySenderHover(SenderButton.IsHovered);

            var messageX = SenderContainer.X + SenderContainer.Width + 10;
            var messageWidth = Math.Max(1, width - messageX - 10);

            Message.Position = new ScalableVector2(messageX, VerticalPadding);
            Message.MaxWidth = messageWidth;
            Message.Text = item.Message ?? string.Empty;

            Height = Math.Max(40, Message.Height + VerticalPadding * 2);
        }

        private void ApplySenderHover(bool hovered)
        {
            SenderBadge.Tint = hovered ? Darken(Color.White) : Color.White;
            SenderClan.Tint = hovered ? Darken(SenderClanColor) : SenderClanColor;
            Sender.Tint = hovered ? Darken(SenderColor) : SenderColor;
        }

        private static Color Darken(Color color) => new Color(color.R / 2, color.G / 2, color.B / 2);

        private static TextureRegion? GetBadge(UserGroups groups)
        {
            if (groups.HasFlag(UserGroups.Swan))
                return UserGroupAssets.Get(GlobalUserGroup.Swan);

            if (groups.HasFlag(UserGroups.Developer))
                return UserGroupAssets.Get(GlobalUserGroup.Developer);

            if (groups.HasFlag(UserGroups.GraphicDesigner))
                return UserGroupAssets.Get(GlobalUserGroup.GraphicDesigner);

            if (groups.HasFlag(UserGroups.Bot))
                return UserGroupAssets.Get(GlobalUserGroup.Bot);

            if (groups.HasFlag(UserGroups.Admin))
                return UserGroupAssets.Get(GlobalUserGroup.Administrator);

            if (groups.HasFlag(UserGroups.Moderator))
                return UserGroupAssets.Get(GlobalUserGroup.Moderator);

            if (groups.HasFlag(UserGroups.TrialRankingSupervisor))
                return UserGroupAssets.Get(GlobalUserGroup.TrialRankingSupervisor);

            if (groups.HasFlag(UserGroups.RankingSupervisor))
                return UserGroupAssets.Get(GlobalUserGroup.RankingSupervisor);

            if (groups.HasFlag(UserGroups.HeadRankingSupervisor))
                return UserGroupAssets.Get(GlobalUserGroup.HeadRankingSupervisor);

            if (groups.HasFlag(UserGroups.Contributor))
                return UserGroupAssets.Get(GlobalUserGroup.Contributor);

            if (groups.HasFlag(UserGroups.Donator))
                return UserGroupAssets.Get(GlobalUserGroup.Donator);

            if (groups.HasFlag(UserGroups.CommunityManager))
                return UserGroupAssets.Get(GlobalUserGroup.CommunityManager);

            return null;
        }

        public override void Destroy()
        {
            SenderMenuRequested = null;
            TimestampTooltipRegistration.Dispose();
            base.Destroy();
        }

        private sealed class ViewportRoundedButton : RoundedButton
        {
            private ScrollContainer Viewport { get; }

            public ViewportRoundedButton(ScrollContainer viewport) => Viewport = viewport;

            protected override bool IsMouseInClickArea() =>
                base.IsMouseInClickArea() &&
                GraphicsHelper.RectangleContains(Viewport.ScreenRectangle, MouseManager.CurrentState.Position);
        }
    }
}
