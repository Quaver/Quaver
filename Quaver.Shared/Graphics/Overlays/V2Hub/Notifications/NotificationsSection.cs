using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;

public class NotificationsSection : Container
{
    private SkinV2HubConfig HubConfig { get; }
    private SkinStoreV2Lease Skin { get; }
    private SkinV2HubSectionConfig SectionConfig => HubConfig.Sections;
    private SkinV2HubNotificationsConfig Config => HubConfig.Notifications;
    private bool PreviewMode { get; }

    private FlexContainer Layout { get; set; }

    private RoundedButton RecentNotification { get; set; }
    private RoundedButton AllNotification { get; set; }
    private RoundedButton ClearButton { get; set; }

    private NotificationList Scroll { get; set; }
    private SpriteTextPlus EmptyMessage { get; set; }
    public NotificationFeed CurrentFeed { get; set; }
    internal Sprite Header { get; private set; }

    public NotificationsSection(ScalableVector2 size, SkinStoreV2Lease skin, SkinV2Config rootConfig, bool previewMode = false)
    {
        Skin = skin;
        HubConfig = rootConfig.Screens.Hub;
        PreviewMode = previewMode;
        Size = size;
        CreateLayout();
        CreateHeader();
        CreateContent();

        NotificationManager.NotificationMissed += OnNotificationMissed;
        NotificationManager.NotificationShown += OnNotificationShown;
        NotificationManager.NotificationViewed += OnNotificationViewed;

        Layout.RefreshLayout();
    }

    private void CreateLayout()
    {
        var padding = SectionConfig.ContentPadding;
        Layout = new FlexContainer
        {
            Parent = this,
            Position = new ScalableVector2(padding, padding),
            Size = new ScalableVector2(Width - padding, Height - padding * 2),
            Direction = FlexDirection.Column,
            AlignItems = FlexAlignItems.Stretch
        };
    }

    private void CreateHeader()
    {
        var section = SectionConfig;
        Header = new Sprite
        {
            Parent = Layout,
            Size = new ScalableVector2(Layout.Width, section.HeaderHeight),
            Tint = SkinV2Color.Parse(section.BackgroundColor)
        };
        Layout.SetItemOptions(Header, new FlexItemOptions { Basis = section.HeaderHeight, Shrink = 0 });

        var headerLayout = new FlexContainer
        {
            Parent = Header,
            Size = new ScalableVector2(Header.Width - section.ContentPadding, Header.Height - section.ContentPadding * 2),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustifyContent.SpaceBetween
        };

        var buttonLayout = new FlexContainer
        {
            Parent = headerLayout,
            Size = new ScalableVector2(Config.FeedButtonWidth * 2, section.ControlHeight),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.FlexStart,
            JustifyContent = FlexJustifyContent.FlexStart,
            ColumnGap = 0
        };

        // Connect both button together
        var cornerRadius = section.CornerRadius;
        RecentNotification = new RoundedButton
        {
            Parent = buttonLayout,
            Size = new ScalableVector2(Config.FeedButtonWidth, section.ControlHeight),
            CornerRadii = new RoundedRectCornerRadii(cornerRadius, 0, 0, cornerRadius), // used to remove the rounded edges on connecting sides
            Tint = SkinV2Color.Parse(section.SelectedControlColor)
        };
        UpdateNewCountLabel();

        AllNotification = new RoundedButton
        {
            Parent = buttonLayout,
            Size = new ScalableVector2(Config.FeedButtonWidth, section.ControlHeight),
            CornerRadii = new RoundedRectCornerRadii(0, cornerRadius, cornerRadius, 0),
            Tint = SkinV2Color.Parse(section.ControlColor)
        };
        AllNotification.SetLabel(FontManager.GetWobbleFont(section.Font), LocalizationManager.Get("Screen_Hub_NotificationAll"), section.FontSize, SkinV2Color.Parse(section.TextColor));

        ClearButton = new RoundedButton((sender, args) => ClearAllNotifications())
        {
            Parent = headerLayout,
            Size = new ScalableVector2(section.ClearButtonWidth, section.ControlHeight),
            CornerRadius = section.CornerRadius,
            Tint = SkinV2Color.Parse(section.ControlColor)
        };
        ClearButton.SetLabel(FontManager.GetWobbleFont(section.Font), LocalizationManager.Get("Screen_Hub_NotificationClearAll"), section.FontSize, SkinV2Color.Parse(section.TextColor));
        ClearButton.SetIcon(Skin.LoadTexture(section.ClearIcon, UserInterface.HubNotificationsClearIcon), new Vector2(section.IconSize, section.IconSize));
    }

    private void CreateContent()
    {
        var availableHeight = Height - SectionConfig.HeaderHeight - SectionConfig.ContentPadding * 2;
        Scroll = new NotificationList(new List<NotificationInfo>(NotificationManager.NewNotificationsFeed), (int) Math.Ceiling(Math.Max(0, availableHeight) / 80) + 3, 0, new ScalableVector2(Width, 1), new ScalableVector2(Width, 1))
        {
            Parent = Layout,
            Tint = SkinV2Color.Parse(SectionConfig.BackgroundColor),
            EasingType = Easing.OutQuint,
            TimeToCompleteScroll = 1200,
            ScrollSpeed = 220,
            InputEnabled = true
        };
        V2Scrollbar.Apply(Scroll);
        Layout.SetItemOptions(Scroll, new FlexItemOptions { Basis = 0, Grow = 1 });
        Scroll.OnRowClicked += OnNotificationRowClicked;

        CurrentFeed = NotificationFeed.Recent;
        EmptyMessage = new SpriteTextPlus(FontManager.GetWobbleFont(SectionConfig.Font), "", SectionConfig.EmptyMessageFontSize)
        {
            Parent = Scroll,
            Alignment = Alignment.MidCenter,
            TextAlignment = TextAlignment.Center,
            MaxWidth = Scroll.Width - SectionConfig.ContentPadding * 4,
            Tint = SkinV2Color.Parse(SectionConfig.TextColor),
            UsePreviousSpriteBatchOptions = true
        };
        UpdateEmptyMessage();

        RecentNotification.Clicked += (sender, args) => SwitchFeed(NotificationFeed.Recent);
        AllNotification.Clicked += (sender, args) => SwitchFeed(NotificationFeed.History);
    }

    private void UpdateEmptyMessage()
    {
        EmptyMessage.Text = LocalizationManager.Get("Screen_Hub_NotificationEmpty");
        EmptyMessage.Visible = Scroll.AvailableItems.Count == 0;
    }

    private void OnNotificationMissed(object sender, NotificationMissedEventArgs e)
    {
        if (!PreviewMode && Visible && GameBase.Game is QuaverGame game)
            game.OnlineHub?.MarkSectionAsRead(OnlineHubSectionType.Notifications);

        ScheduleFeedRefresh();
    }

    private void OnNotificationShown(NotificationInfo info)
    {
        if (PreviewMode || !Visible || CurrentFeed != NotificationFeed.Recent)
            return;

        if (NotificationManager.StoreShownNotification(info))
            ScheduleFeedRefresh();
    }

    private void OnNotificationViewed(NotificationInfo info) => ScheduleFeedRefresh();

    private void ScheduleFeedRefresh() => AddScheduledUpdate(() =>
    {
        UpdateNewCountLabel();

        var feed = CurrentFeed == NotificationFeed.Recent ? NotificationManager.NewNotificationsFeed : NotificationManager.HistoryNotificationsFeed;
        Scroll.RefreshFeed(feed);
        UpdateEmptyMessage();
    });

    private void OnNotificationRowClicked(NotificationInfo item)
    {
        if (PreviewMode)
            return;

        var feed = CurrentFeed == NotificationFeed.Recent ? NotificationManager.NewNotificationsFeed : NotificationManager.HistoryNotificationsFeed;

        if (!feed.Remove(item))
            return;

        Scroll.ChangeFeed(feed);
        UpdateEmptyMessage();

        if (CurrentFeed == NotificationFeed.Recent)
            UpdateNewCountLabel();
    }

    private void SwitchFeed(NotificationFeed feedType)
    {
        switch (feedType)
        {
            case NotificationFeed.Recent:
                if (CurrentFeed == NotificationFeed.Recent)
                    return;
                Scroll.ChangeFeed(new List<NotificationInfo>(NotificationManager.NewNotificationsFeed));
                RecentNotification.Tint = SkinV2Color.Parse(SectionConfig.SelectedControlColor);
                AllNotification.Tint = SkinV2Color.Parse(SectionConfig.ControlColor);
                break;
            case NotificationFeed.History:
                if (CurrentFeed == NotificationFeed.History)
                    return;

                SetNewNotificationToViewed();

                Scroll.ChangeFeed(new List<NotificationInfo>(NotificationManager.HistoryNotificationsFeed));
                RecentNotification.Tint = SkinV2Color.Parse(SectionConfig.ControlColor);
                AllNotification.Tint = SkinV2Color.Parse(SectionConfig.SelectedControlColor);
                break;
        }
        CurrentFeed = feedType;
        UpdateEmptyMessage();
    }


    public void SetNewNotificationToViewed()
    {
        if (PreviewMode || CurrentFeed != NotificationFeed.Recent)
            return;

        var notificationToRemove = new HashSet<NotificationInfo>(Scroll.AvailableItems);
        NotificationManager.NewNotificationsFeed.RemoveAll(notificationToRemove.Contains);

        Scroll.ChangeFeed(new List<NotificationInfo>(NotificationManager.NewNotificationsFeed));
        UpdateEmptyMessage();

        UpdateNewCountLabel();
    }
    private void UpdateNewCountLabel()
    {
        RecentNotification.SetLabel(FontManager.GetWobbleFont(SectionConfig.Font), LocalizationManager.Get("Screen_Hub_NotificationRecent", NotificationManager.NewNotificationsFeed.Count), SectionConfig.FontSize, SkinV2Color.Parse(SectionConfig.TextColor));
    }

    private void ClearAllNotifications()
    {
        if (PreviewMode)
            return;

        NotificationManager.NewNotificationsFeed.Clear();
        NotificationManager.HistoryNotificationsFeed.Clear();

        if (GameBase.Game is QuaverGame game)
            game.OnlineHub?.MarkSectionAsRead(OnlineHubSectionType.Notifications);

        Scroll.ChangeFeed(new List<NotificationInfo>());
        UpdateEmptyMessage();

        UpdateNewCountLabel();
    }

    public void Deactivate()
    {

    }

    internal void ResizeHeight(float height)
    {
        if (Math.Abs(Height - height) < 0.01f)
            return;

        Height = height;
        Layout.Height = Math.Max(1, height - SectionConfig.ContentPadding * 2);
        Layout.RefreshLayout();
    }

    public override void Destroy()
    {
        NotificationManager.NotificationMissed -= OnNotificationMissed;
        NotificationManager.NotificationShown -= OnNotificationShown;
        NotificationManager.NotificationViewed -= OnNotificationViewed;

        base.Destroy();
    }

    public enum NotificationFeed
    {
        Recent,
        History
    }
}
