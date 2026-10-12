using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;

/// <summary>
///     Shared visual card used by both on-screen notifications and notification hub rows.
/// </summary>
public class NotificationCard : Container
{
    public const float DefaultWidth = 400;
    public const float MinimumHeight = 64;

    private const float BarWidth = 8;
    private const float BarGap = 8;
    private const float ContentPadding = 12;
    private const float ImageSlotSize = 40;
    private const float IconSize = 34;
    private const float ImageTextGap = 10;
    private const float TextGap = 2;
    private const float TimestampHeight = 20;
    private const float FooterGap = 10;

    private bool ShowTimestamp { get; }

    private NotificationInfo Item { get; set; }

    private Sprite Bar { get; }

    private ViewportRoundedButton Row { get; }

    private Container ImageSlot { get; }

    private Sprite Icon { get; }

    private SpriteAlphaMaskBlend Avatar { get; }

    private SpriteTextPlus Title { get; }

    private SpriteTextPlus Description { get; }

    private FlexContainer TimestampLayout { get; }

    private SpriteTextPlus Timestamp { get; }

    public Container Footer { get; }

    public event Action<NotificationInfo, object, EventArgs> Clicked;

    public bool IsCardHovered => Row.IsHovered;

    public NotificationCard(NotificationInfo item, float width, bool showTimestamp, ScrollContainer viewport = null, bool allowInputWhenDialogOpen = false)
    {
        ShowTimestamp = showTimestamp;
        Size = new ScalableVector2(width, MinimumHeight);

        Bar = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(BarWidth, Height),
            Image = RoundedRectTextureCache.Get(BarWidth, Height, BarWidth / 2)
        };

        Row = new ViewportRoundedButton(viewport)
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            Size = new ScalableVector2(GetRowWidth(width), Height),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.FromHex("#181E25"),
            PerformHoverFade = false,
            AllowInputWhenDialogOpen = allowInputWhenDialogOpen
        };
        Row.Clicked += (sender, args) => Clicked?.Invoke(Item, sender, args);
        Row.Hovered += (sender, args) => Row.Tint = ColorHelper.FromHex("#3D4B64");
        Row.LeftHover += (sender, args) => Row.Tint = ColorHelper.FromHex("#181E25");

        ImageSlot = new Container
        {
            Parent = Row,
            Size = new ScalableVector2(ImageSlotSize, ImageSlotSize)
        };

        Icon = new Sprite
        {
            Parent = ImageSlot,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(IconSize, IconSize),
            UsePreviousSpriteBatchOptions = true
        };

        Avatar = new SpriteAlphaMaskBlend
        {
            Parent = ImageSlot,
            Size = new ScalableVector2(ImageSlotSize, ImageSlotSize),
            Image = UserInterface.UnknownAvatar,
            Visible = false,
            UsePreviousSpriteBatchOptions = true
        };

        Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 18)
        {
            Parent = Row,
            Tint = Color.White,
            UsePreviousSpriteBatchOptions = true
        };

        Description = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 16)
        {
            Parent = Row,
            Tint = ColorHelper.FromHex("#A0A6B1"),
            Visible = false,
            UsePreviousSpriteBatchOptions = true
        };

        TimestampLayout = new FlexContainer
        {
            Parent = Row,
            Size = new ScalableVector2(100, TimestampHeight),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 5,
            Visible = showTimestamp,
            UpdateWhenInvisible = false
        };

        var clock = new Sprite
        {
            Parent = TimestampLayout,
            Size = new ScalableVector2(12, 12),
            Image = UserInterface.ClockV2,
            Tint = ColorHelper.FromHex("#7E8793"),
            UsePreviousSpriteBatchOptions = true
        };
        TimestampLayout.SetItemOptions(clock, new FlexItemOptions { Basis = clock.Width, Shrink = 0 });

        Timestamp = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 14)
        {
            Parent = TimestampLayout,
            Tint = ColorHelper.FromHex("#7E8793"),
            UsePreviousSpriteBatchOptions = true
        };
        TimestampLayout.SetItemOptions(Timestamp, new FlexItemOptions { Shrink = 0 });

        Footer = new Container
        {
            Parent = Row,
            Visible = false,
            UpdateWhenInvisible = false
        };

        SteamManager.SteamUserAvatarLoaded += OnSteamUserAvatarLoaded;
        UpdateContent(item);
    }

    public void UpdateContent(NotificationInfo item, float footerHeight = 0, float minimumHeight = MinimumHeight)
    {
        Item = item;

        var showAvatar = ShouldDisplayAvatar(item);
        Icon.Visible = !showAvatar;
        Avatar.Visible = showAvatar;

        Bar.Tint = GetColor(item.Level);
        Icon.Image = GetIconTexture(item.Level);

        Title.Text = GetTitle(item);
        Description.Text = GetDescription(item);
        Description.Visible = !string.IsNullOrWhiteSpace(Description.Text);

        Timestamp.Text = item.CreatedAt.ToLocalTime().ToString("hh:mm tt");
        TimestampLayout.RefreshLayout();

        Footer.Visible = footerHeight > 0;

        if (showAvatar)
            UpdateAvatar(item.SenderSteamId);

        RefreshLayout(footerHeight, minimumHeight);
    }

    public void ResizeWidth(float width, float footerHeight = 0, float minimumHeight = MinimumHeight)
    {
        Width = width;
        RefreshLayout(footerHeight, minimumHeight);
    }

    private void RefreshLayout(float footerHeight, float minimumHeight)
    {
        var rowWidth = GetRowWidth(Width);
        var textX = ContentPadding + ImageSlotSize + ImageTextGap;
        var textWidth = Math.Max(1, rowWidth - textX - ContentPadding);

        Title.MaxWidth = textWidth;
        Description.MaxWidth = textWidth;

        var textHeight = Title.Height;
        if (Description.Visible)
            textHeight += TextGap + Description.Height;
        if (ShowTimestamp)
            textHeight += TextGap + TimestampHeight;

        var detailsHeight = textHeight;
        if (footerHeight > 0)
            detailsHeight += FooterGap + footerHeight;

        var contentHeight = Math.Max(ImageSlotSize, detailsHeight);
        var height = Math.Max(minimumHeight, contentHeight + ContentPadding * 2);

        Size = new ScalableVector2(Width, height);
        Bar.Size = new ScalableVector2(BarWidth, height);
        Bar.Image = RoundedRectTextureCache.Get(BarWidth, height, BarWidth / 2);
        Row.Size = new ScalableVector2(rowWidth, height);

        var y = (height - contentHeight) / 2f;
        ImageSlot.Position = new ScalableVector2(ContentPadding, Avatar.Visible ? y : (height - ImageSlotSize) / 2f);

        Title.Position = new ScalableVector2(textX, y);
        y += Title.Height;

        if (Description.Visible)
        {
            y += TextGap;
            Description.Position = new ScalableVector2(textX, y);
            y += Description.Height;
        }

        if (ShowTimestamp)
        {
            y += TextGap;
            TimestampLayout.Position = new ScalableVector2(textX, y);
            y += TimestampHeight;
        }

        if (footerHeight > 0)
        {
            y += FooterGap;
            Footer.Position = new ScalableVector2(textX, y);
            Footer.Size = new ScalableVector2(textWidth, footerHeight);
        }
    }

    public static float MeasureHeight(NotificationInfo item, float width, bool showTimestamp, float footerHeight = 0)
    {
        var rowWidth = GetRowWidth(width);
        var textX = ContentPadding + ImageSlotSize + ImageTextGap;
        var textWidth = Math.Max(1, rowWidth - textX - ContentPadding);

        var textHeight = MeasureTextHeight(GetTitle(item), 18, textWidth);
        var description = GetDescription(item);
        if (!string.IsNullOrWhiteSpace(description))
            textHeight += TextGap + MeasureTextHeight(description, 16, textWidth);
        if (showTimestamp)
            textHeight += TextGap + TimestampHeight;
        if (footerHeight > 0)
            textHeight += FooterGap + footerHeight;

        return Math.Max(MinimumHeight, Math.Max(ImageSlotSize, textHeight) + ContentPadding * 2);
    }

    private static float GetRowWidth(float width) => Math.Max(1, width - BarWidth - BarGap);

    private static string GetTitle(NotificationInfo item) =>
        item.Type == NotificationType.MultiplayerInvite && !string.IsNullOrWhiteSpace(item.SenderName)
            ? item.SenderName : item.Text ?? string.Empty;

    private static string GetDescription(NotificationInfo item) =>
        item.Type == NotificationType.DirectMessage || item.Type == NotificationType.MultiplayerInvite
            ? item.DetailText ?? string.Empty : string.Empty;

    private static bool ShouldDisplayAvatar(NotificationInfo item) =>
        item.Type == NotificationType.DirectMessage || item.Type == NotificationType.MultiplayerInvite;

    private static float MeasureTextHeight(string text, int fontSize, float maxWidth)
    {
        var measurement = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), text, fontSize, subscribeToFontChanges: false)
        {
            MaxWidth = maxWidth
        };
        var height = measurement.Height;
        measurement.Destroy();
        return height;
    }

    private void UpdateAvatar(long senderSteamId)
    {
        var steamId = (ulong)senderSteamId;

        if (steamId != 0 && SteamManager.UserAvatars != null &&
            SteamManager.UserAvatars.TryGetValue(steamId, out var texture))
        {
            ScheduleAvatarUpdate(texture, steamId);
            return;
        }

        ScheduleAvatarUpdate(UserInterface.UnknownAvatar, steamId);
        if (steamId != 0)
            SteamManager.SendAvatarRetrievalRequest(steamId);
    }

    private void ScheduleAvatarUpdate(Texture2D texture, ulong steamId)
    {
        if (texture == null || texture.IsDisposed)
            return;

        GameBase.Game.ScheduleRenderTargetDraw(() =>
        {
            if (IsDisposed || Avatar.IsDisposed || texture.IsDisposed || !ShouldDisplayAvatar(Item) || (ulong)Item.SenderSteamId != steamId)
                return;

            Avatar.Image = Avatar.PerformBlend(texture, UserInterface.ResultsAvatarMask);
        });
    }

    private void OnSteamUserAvatarLoaded(object sender, SteamAvatarLoadedEventArgs args)
    {
        AddScheduledUpdate(() =>
        {
            if (IsDisposed || !ShouldDisplayAvatar(Item) || (ulong)Item.SenderSteamId != args.SteamId)
                return;

            ScheduleAvatarUpdate(args.Texture, args.SteamId);
        });
    }

    private static Color GetColor(NotificationLevel level)
    {
        return level switch
        {
            NotificationLevel.Info => ColorHelper.FromHex("#0FBAE5"),
            NotificationLevel.Error => ColorHelper.FromHex("#F9645D"),
            NotificationLevel.Warning => ColorHelper.FromHex("#E9B736"),
            NotificationLevel.Success => ColorHelper.FromHex("#27B06E"),
            _ => ColorHelper.FromHex("#0FBAE5")
        };
    }

    private static Texture2D GetIconTexture(NotificationLevel level)
    {
        return level switch
        {
            NotificationLevel.Info => UserInterface.NotificationInfoV2,
            NotificationLevel.Error => UserInterface.NotificationErrorV2,
            NotificationLevel.Warning => UserInterface.NotificationWarningV2,
            NotificationLevel.Success => UserInterface.NotificationSuccessV2,
            _ => UserInterface.NotificationInfoV2
        };
    }

    public override void Destroy()
    {
        SteamManager.SteamUserAvatarLoaded -= OnSteamUserAvatarLoaded;
        base.Destroy();
    }

    private sealed class ViewportRoundedButton : RoundedButton
    {
        private ScrollContainer Viewport { get; }

        public ViewportRoundedButton(ScrollContainer viewport) => Viewport = viewport;

        protected override bool IsMouseInClickArea() =>
            base.IsMouseInClickArea() &&
            (Viewport == null || GraphicsHelper.RectangleContains(Viewport.ScreenRectangle, MouseManager.CurrentState.Position));
    }
}
