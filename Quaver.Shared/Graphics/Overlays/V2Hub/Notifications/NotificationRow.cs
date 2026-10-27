using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
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

public class NotificationRow : PoolableSprite<NotificationInfo>
{
    public override int HEIGHT => Container == null ? 88 : GetHeight(Item);

    public static int GetHeight(NotificationInfo item) => item.Type == NotificationType.MultiplayerInvite ? 132 : 88;

    private ViewportRoundedButton Row { get; set; }
    private ViewportRoundedButton CloseButton { get; set; }
    
    private Sprite Bar { get; set; }
    private Container ImageSlot { get; set; }
    private Sprite Icon { get; set; }
    private SpriteAlphaMaskBlend Avatar { get; set; }
    private Sprite ClockIcon { get; set; }
    private MarqueeSpriteText Title { get; set; }
    private SpriteTextPlus Time { get; set; }
    private MarqueeSpriteText Description { get; set; }
    private ViewportRoundedButton AcceptButton { get; set; }
    private ViewportRoundedButton DenyButton { get; set; }

    
    private FlexContainer Layout { get; set; }
    private FlexContainer ContentLayout { get; set; }
    private FlexContainer DetailsLayout { get; set; }
    private FlexContainer TextLayout { get; set; }
    private FlexContainer HeaderLayout { get; set; }
    private FlexContainer TimeLayout { get; set; }
    private FlexContainer ActionsLayout { get; set; }

    private FlexItemOptions ImageSlotOptions { get; set; }
    private FlexItemOptions DetailsLayoutOptions { get; set; }
    private FlexItemOptions TextLayoutOptions { get; set; }
    private FlexItemOptions ActionsLayoutOptions { get; set; }
    
    public event Action<NotificationInfo> OnClick;

    public bool IsCardHovered => Row.IsHovered || CloseButton.IsHovered;

    public NotificationRow(PoolableScrollContainer<NotificationInfo> container, NotificationInfo item, int index) : base(container, item, index)
    {
        Size = new ScalableVector2(container?.Width ?? 716, container == null ? 88 : HEIGHT);

        // Container is null when the notification is used outside of the hub
        if (container == null)
            Alpha = 0;

        CreateRow();
        CreateIcon();
        CreateTitle();
        CreateClockIcon();
        CreateTime();
        CreateDescription();
        CreateCloseButton();
        CreateActionsLayout();

        Layout.RefreshLayout();
        ContentLayout.RefreshLayout();
        DetailsLayout.RefreshLayout();
        TextLayout.RefreshLayout();
        HeaderLayout.RefreshLayout();
        TimeLayout.RefreshLayout();
        ActionsLayout.RefreshLayout();

        if (container != null || item.Type == NotificationType.DirectMessage)
            SteamManager.SteamUserAvatarLoaded += OnSteamUserAvatarLoaded;
    }

    public override void UpdateContent(NotificationInfo item, int index)
    {
        Item = item;
        Index = index;

        var isInviteNotification = Container != null && item.Type == NotificationType.MultiplayerInvite;
        var showAvatar = ShouldDisplayAvatar(item);
        Icon.Visible = !showAvatar;
        Avatar.Visible = showAvatar;

        // Recalc components size based on notification type (pain)
        var totalHeight = isInviteNotification ? GetHeight(item) : 88;
        Size = new ScalableVector2(Width, totalHeight);

        var rowHeight = Container == null ? totalHeight : totalHeight - 10;
        Layout.Height = rowHeight;
        Bar.Height = rowHeight;
        Row.Height = rowHeight;

        var imageWidth = showAvatar ? 48 : 34;
        ImageSlot.Size = new ScalableVector2(imageWidth, imageWidth);
        ImageSlotOptions.Basis = imageWidth;
        Avatar.Size = new ScalableVector2(imageWidth, imageWidth);

        var detailsWidth = isInviteNotification ? Row.Width - 54 - imageWidth - 15 : Row.Width - 50 - imageWidth - 50;
        Title.Width = detailsWidth - 110;
        Description.Width = detailsWidth;
        Bar.Image = RoundedRectTextureCache.Get(12, rowHeight, 6);

        // Layout recalc
        ContentLayout.Position = new ScalableVector2(isInviteNotification ? 14 : 10, 10);
        ContentLayout.Size = new ScalableVector2(Row.Width - (isInviteNotification ? 54 : 50), rowHeight - 20);
        ContentLayout.AlignItems = isInviteNotification ? FlexAlignItems.FlexStart : FlexAlignItems.Center;

        DetailsLayout.Size = new ScalableVector2(detailsWidth, isInviteNotification ? 102 : 50);
        DetailsLayoutOptions.Basis = detailsWidth;
        DetailsLayout.JustifyContent = isInviteNotification ? FlexJustifyContent.FlexStart : FlexJustifyContent.Center;
        DetailsLayout.RowGap = isInviteNotification ? 2 : 0;

        TextLayout.Size = new ScalableVector2(detailsWidth, isInviteNotification ? HeaderLayout.Height + TextLayout.RowGap + Description.Height : 50);
        TextLayoutOptions.Basis = TextLayout.Height;

        HeaderLayout.Width = detailsWidth;

        ActionsLayout.Visible = isInviteNotification;
        ActionsLayout.Size = new ScalableVector2(detailsWidth, isInviteNotification ? 40 : 0);
        ActionsLayoutOptions.Basis = isInviteNotification ? 40 : 0;

        Bar.Tint = GetColor(item.Level);
        Icon.Image = GetIconTexture(item.Level);

        Title.TextSprite.Text = isInviteNotification ? item.SenderName ?? "" : item.Text;
        Title.ResetPosition();
        Description.TextSprite.Text = isInviteNotification || item.Type == NotificationType.DirectMessage ? item.DetailText ?? item.Text : "";
        Description.ResetPosition();

        Time.Text = item.CreatedAt.ToLocalTime().ToString("hh:mm tt");

        if (showAvatar)
            UpdateAvatar(item.SenderSteamId);

        if (!isInviteNotification)
        {
            AcceptButton.ResetInteractionState();
            DenyButton.ResetInteractionState();
        }

        Layout.RefreshLayout();
        ContentLayout.RefreshLayout();
        DetailsLayout.RefreshLayout();
        TextLayout.RefreshLayout();
        HeaderLayout.RefreshLayout();
        TimeLayout.RefreshLayout();
        ActionsLayout.RefreshLayout();
    }

    private void CreateRow()
    {
        Layout = new FlexContainer
        {
            Parent = this,
            Size = new ScalableVector2(716, 78),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 10
        };

        Bar = new Sprite
        {
            Parent = Layout,
            Size = new ScalableVector2(12, 78),
            Image = RoundedRectTextureCache.Get(12, 78, 6),
            Tint = ColorHelper.FromHex("#32FF9F")
        };
        Layout.SetItemOptions(Bar, new FlexItemOptions { Basis = Bar.Width, Shrink = 0 });

        Row = new ViewportRoundedButton(Container)
        {
            Parent = Layout,
            Size = new ScalableVector2(694, 78),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.FromHex("#181E25"),
            PerformHoverFade = false,
            AllowInputWhenDialogOpen = Container == null
        };
        Layout.SetItemOptions(Row, new FlexItemOptions { Basis = Row.Width, Shrink = 0 });

        Row.Clicked += (sender, args) =>
        {
            if (Container != null && Item.Type != NotificationType.General)
                return;

            var clickedItem = Item;
            OnClick?.Invoke(clickedItem);
            clickedItem.ClickAction?.Invoke(sender, args);
        };

        Row.Hovered += (s, e) =>
        {
            Row.Tint = ColorHelper.FromHex("#3D4B64");
            Title.IsActive = true;
        };
        Row.LeftHover += (s, e) =>
        {
            Row.Tint = ColorHelper.FromHex("#181E25");
            Title.IsActive = false;
        };
        
        ContentLayout = new FlexContainer
        {
            Parent = Row,
            Position = new ScalableVector2(10, 10),
            Size = new ScalableVector2(Row.Width - 50, Row.Height - 20),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 15,
            UpdateWhenInvisible = false
        };
    }

    private void CreateIcon()
    {
        ImageSlot = new Container
        {
            Parent = ContentLayout,
            Size = new ScalableVector2(34, 34)
        };
        ImageSlotOptions = new FlexItemOptions { Basis = ImageSlot.Width, Shrink = 0 };
        ContentLayout.SetItemOptions(ImageSlot, ImageSlotOptions);

        Icon = new Sprite
        {
            Parent = ImageSlot,
            Size = new ScalableVector2(34, 34),
            Image = UserInterface.NotificationInfo,
            UsePreviousSpriteBatchOptions = true
        };

        Avatar = new SpriteAlphaMaskBlend
        {
            Parent = ImageSlot,
            Size = new ScalableVector2(48, 48),
            Image = UserInterface.UnknownAvatar,
            Visible = false,
            UsePreviousSpriteBatchOptions = true
        };
        
        DetailsLayout = new FlexContainer
        {
            Parent = ContentLayout,
            Size = new ScalableVector2(ContentLayout.Width - Icon.Width - 20 - 30, 50),
            Direction = FlexDirection.Column,
            JustifyContent = FlexJustifyContent.Center,
            AlignItems = FlexAlignItems.Stretch,
            RowGap = 0
        };
        DetailsLayoutOptions = new FlexItemOptions { Basis = DetailsLayout.Width, Shrink = 0 };
        ContentLayout.SetItemOptions(DetailsLayout, DetailsLayoutOptions);
        
        TextLayout = new FlexContainer
        {
            Parent = DetailsLayout,
            Size = new ScalableVector2(DetailsLayout.Width, 50),
            Direction = FlexDirection.Column,
            AlignItems = FlexAlignItems.Stretch,
            RowGap = 2
        };
        TextLayoutOptions = new FlexItemOptions { Basis = TextLayout.Height, Shrink = 0 };
        DetailsLayout.SetItemOptions(TextLayout, TextLayoutOptions);

        HeaderLayout = new FlexContainer
        {
            Parent = TextLayout,
            Size = new ScalableVector2(DetailsLayout.Width, 24),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 5
        };
        TextLayout.SetItemOptions(HeaderLayout, new FlexItemOptions { Basis = 24, Shrink = 0 });
    }

    private void CreateTitle()
    {
        Title = new MarqueeSpriteText(FontManager.GetWobbleFont(Fonts.InterBold), "", 20, HeaderLayout.Width - 110)
        {
            Parent = HeaderLayout,
            UsePreviousSpriteBatchOptions = true
        };
        Title.TextSprite.Tint = Color.White;
        HeaderLayout.SetItemOptions(Title, new FlexItemOptions { Shrink = 0 });
    }

    private void CreateClockIcon()
    {
        TimeLayout = new FlexContainer
        {
            Parent = HeaderLayout,
            Size = new ScalableVector2(100, 20),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 5
        };
        HeaderLayout.SetItemOptions(TimeLayout, new FlexItemOptions { Basis = TimeLayout.Width, Shrink = 0 });

        ClockIcon = new Sprite
        {
            Parent = TimeLayout,
            Size = new ScalableVector2(12, 12),
            Image = UserInterface.ClockV2,
            Tint = ColorHelper.FromHex("#7E8793"),
            UsePreviousSpriteBatchOptions = true
        };
        TimeLayout.SetItemOptions(ClockIcon, new FlexItemOptions { Basis = ClockIcon.Width, Shrink = 0 });
    }

    private void CreateTime()
    {
        Time = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "", 14)
        {
            Parent = TimeLayout,
            Tint = ColorHelper.FromHex("#7E8793"),
            UsePreviousSpriteBatchOptions = true
        };
        TimeLayout.SetItemOptions(Time, new FlexItemOptions { Shrink = 0 });
    }

    private void CreateDescription()
    {
        Description = new MarqueeSpriteText(FontManager.GetWobbleFont(Fonts.InterBold), "", 18, DetailsLayout.Width)
        {
            Parent = TextLayout,
            IsActive = false,
            UsePreviousSpriteBatchOptions = true
        };
        Description.TextSprite.Tint = ColorHelper.FromHex("#A0A6B1");
        TextLayout.SetItemOptions(Description, new FlexItemOptions { Shrink = 0, AlignSelf = FlexAlignSelf.FlexStart });

        Row.Hovered += (s, e) => Description.IsActive = true;
        Row.LeftHover += (s, e) => Description.IsActive = false;
    }
    
    private void CreateCloseButton()
    {
        CloseButton = new ViewportRoundedButton(Container)
        {
            Parent = Row,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(-10, 10),
            Size = new ScalableVector2(20, 20),
            CornerRadius = 10,
            Tint = ColorHelper.FromHex("#6B83B2"),
            Depth = -1,
            AllowInputWhenDialogOpen = Container == null
        };
        CloseButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "×", 20, ColorHelper.FromHex("#181E25"));

        CloseButton.Clicked += (sender, args) => OnClick?.Invoke(Item);
    }

    private void CreateActionsLayout()
    {
        ActionsLayout = new FlexContainer
        {
            Parent = DetailsLayout,
            Size = new ScalableVector2(DetailsLayout.Width, 0),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = 10,
            Visible = false,
            UpdateWhenInvisible = false
        };
        ActionsLayoutOptions = new FlexItemOptions { Basis = 0, Shrink = 0 };
        DetailsLayout.SetItemOptions(ActionsLayout, ActionsLayoutOptions);

        AcceptButton = new ViewportRoundedButton(Container)
        {
            Parent = ActionsLayout,
            Size = new ScalableVector2(204, 40),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.FromHex("#6B83B2"),
            Depth = -1
        };
        ActionsLayout.SetItemOptions(AcceptButton, new FlexItemOptions { Basis = AcceptButton.Width, Shrink = 0 });
        AcceptButton.Clicked += (sender, args) =>
        {
            var clickedItem = Item;
            clickedItem.ClickAction?.Invoke(sender, args);
            NotificationManager.MarkClicked(clickedItem);
            OnClick?.Invoke(clickedItem);
        };
        AcceptButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_NotificationAccept"), 18, Color.White);

        DenyButton = new ViewportRoundedButton(Container)
        {
            Parent = ActionsLayout,
            Size = new ScalableVector2(204, 40),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.FromHex("#6B83B2"),
            Depth = -1
        };
        DenyButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_NotificationDeny"), 18, Color.White);
        ActionsLayout.SetItemOptions(DenyButton, new FlexItemOptions { Basis = DenyButton.Width, Shrink = 0 });
        DenyButton.Clicked += (sender, args) =>
        {
            var clickedItem = Item;
            NotificationManager.MarkClicked(clickedItem);
            OnClick?.Invoke(clickedItem);
        };
    }

    private void UpdateAvatar(long senderSteamId)
    {
        var steamId = (ulong)senderSteamId;

        if (steamId != 0 && SteamManager.UserAvatars != null &&
            SteamManager.UserAvatars.TryGetValue(steamId, out var texture))
        {
            Avatar.Image = Avatar.PerformBlend(texture, UserInterface.ResultsAvatarMask);
        }
        else
        {
            Avatar.Image = Avatar.PerformBlend(UserInterface.UnknownAvatar, UserInterface.ResultsAvatarMask);

            if (steamId != 0)
                SteamManager.SendAvatarRetrievalRequest(steamId);
        }
    }

    private bool ShouldDisplayAvatar(NotificationInfo item) =>
        item.Type == NotificationType.DirectMessage || Container != null && item.Type == NotificationType.MultiplayerInvite;

    private void OnSteamUserAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
    {
        AddScheduledUpdate(() =>
        {
            if (IsDisposed || !ShouldDisplayAvatar(Item) || (ulong)Item.SenderSteamId != e.SteamId)
                return;

            GameBase.Game.ScheduleRenderTargetDraw(() =>
            {
                if (IsDisposed || !ShouldDisplayAvatar(Item) || (ulong)Item.SenderSteamId != e.SteamId)
                    return;

                Avatar.Image = Avatar.PerformBlend(e.Texture, UserInterface.ResultsAvatarMask);
            });
        });
    }

    private static Color GetColor(NotificationLevel level)
    {
        switch (level)
        {
            case NotificationLevel.Info:
                return ColorHelper.FromHex("#0FBAE5");
            case NotificationLevel.Error:
                return ColorHelper.FromHex("#F9645D");
            case NotificationLevel.Warning:
                return ColorHelper.FromHex("#E9B736");
            case NotificationLevel.Success:
                return ColorHelper.FromHex("#27B06E");
            default:
                return ColorHelper.FromHex("#0FBAE5");
        }
    }

    private static Texture2D GetIconTexture(NotificationLevel level)
    {
        switch (level)
        {
            case NotificationLevel.Info:
                return UserInterface.NotificationInfoV2;
            case NotificationLevel.Error:
                return UserInterface.NotificationErrorV2;
            case NotificationLevel.Warning:
                return UserInterface.NotificationWarningV2;
            case NotificationLevel.Success:
                return UserInterface.NotificationSuccessV2;
            default:
                return UserInterface.NotificationInfoV2;
        }
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