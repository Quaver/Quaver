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
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;

/// <summary>
///     Compact notification displayed outside of the hub on V2 screens.
/// </summary>
public class OnScreenNotificationV2 : Container
{
    public const float NotificationWidth = 400;
    private const float BarWidth = 8;
    private const float ContentPadding = 12;
    private const float ImageSlotSize = 40;
    private const float IconSize = 34;
    private const float TextGap = 2;

    private float RowWidth => NotificationWidth - BarWidth - 8;

    private float TextX => ContentPadding + ImageSlotSize + 10;

    private float TextWidth => RowWidth - TextX - ContentPadding;

    private NotificationInfo Item { get; set; }

    private Sprite Bar { get; }

    private RoundedButton Row { get; }

    private Sprite Icon { get; }

    private SpriteAlphaMaskBlend Avatar { get; }

    private SpriteTextPlus Title { get; }

    private SpriteTextPlus Description { get; }

    public event Action<NotificationInfo> OnClick;

    public bool IsCardHovered => Row.IsHovered;

    public OnScreenNotificationV2(NotificationInfo item)
    {
        Size = new ScalableVector2(NotificationWidth, 64);

        Bar = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(BarWidth, Height),
            Image = RoundedRectTextureCache.Get(BarWidth, Height, BarWidth / 2),
            Tint = GetColor(item.Level)
        };

        Row = new RoundedButton
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            Size = new ScalableVector2(RowWidth, Height),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.FromHex("#181E25"),
            PerformHoverFade = false,
            AllowInputWhenDialogOpen = true
        };
        Row.Clicked += OnRowClicked;
        Row.Hovered += (sender, args) => Row.Tint = ColorHelper.FromHex("#3D4B64");
        Row.LeftHover += (sender, args) => Row.Tint = ColorHelper.FromHex("#181E25");

        Icon = new Sprite
        {
            Parent = Row,
            Alignment = Alignment.MidLeft,
            X = ContentPadding + (ImageSlotSize - IconSize) / 2,
            Size = new ScalableVector2(IconSize, IconSize),
            UsePreviousSpriteBatchOptions = true
        };

        Avatar = new SpriteAlphaMaskBlend
        {
            Parent = Row,
            Alignment = Alignment.TopLeft,
            X = ContentPadding,
            Y = ContentPadding,
            Size = new ScalableVector2(ImageSlotSize, ImageSlotSize),
            Image = UserInterface.UnknownAvatar,
            Visible = false,
            UsePreviousSpriteBatchOptions = true
        };

        Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 18)
        {
            Parent = Row,
            MaxWidth = TextWidth,
            Tint = Color.White,
            UsePreviousSpriteBatchOptions = true
        };

        Description = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), string.Empty, 16)
        {
            Parent = Row,
            MaxWidth = TextWidth,
            Tint = ColorHelper.FromHex("#A0A6B1"),
            Visible = false,
            UsePreviousSpriteBatchOptions = true
        };

        Item = item;
        SteamManager.SteamUserAvatarLoaded += OnSteamUserAvatarLoaded;
    }

    public void UpdateContent(NotificationInfo item)
    {
        Item = item;

        var showAvatar = item.Type == NotificationType.DirectMessage;
        Icon.Visible = !showAvatar;
        Avatar.Visible = showAvatar;

        Bar.Tint = GetColor(item.Level);
        Icon.Image = GetIconTexture(item.Level);
        Title.Text = item.Text;

        Description.Visible = showAvatar && !string.IsNullOrWhiteSpace(item.DetailText);
        Description.Text = Description.Visible ? item.DetailText : string.Empty;

        if (showAvatar)
            UpdateAvatar(item.SenderSteamId);

        RefreshLayout();
    }

    private void RefreshLayout()
    {
        var textHeight = Title.Height + TextGap;
        if (Description.Visible)
            textHeight += Description.Height + TextGap;

        var height = Math.Max(64, Math.Max(ImageSlotSize, textHeight) + ContentPadding * 2);
        Size = new ScalableVector2(NotificationWidth, height);
        Bar.Size = new ScalableVector2(BarWidth, height);
        Bar.Image = RoundedRectTextureCache.Get(BarWidth, height, BarWidth / 2);
        Row.Size = new ScalableVector2(RowWidth, height);

        var y = (height - textHeight) / 2;
        Title.Position = new ScalableVector2(TextX, y);
        y += Title.Height + TextGap;

        if (Description.Visible)
        {
            Description.Position = new ScalableVector2(TextX, y);
            y += Description.Height + TextGap;
        }
    }

    private void OnRowClicked(object sender, EventArgs args)
    {
        var clickedItem = Item;
        clickedItem.ClickAction?.Invoke(sender, args);
        OnClick?.Invoke(clickedItem);
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
            if (IsDisposed || Avatar.IsDisposed || texture.IsDisposed || Item.Type != NotificationType.DirectMessage ||
                (ulong)Item.SenderSteamId != steamId)
                return;

            Avatar.Image = Avatar.PerformBlend(texture, UserInterface.ResultsAvatarMask);
        });
    }

    private void OnSteamUserAvatarLoaded(object sender, SteamAvatarLoadedEventArgs args)
    {
        AddScheduledUpdate(() =>
        {
            if (IsDisposed || Item.Type != NotificationType.DirectMessage || (ulong)Item.SenderSteamId != args.SteamId)
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
}
