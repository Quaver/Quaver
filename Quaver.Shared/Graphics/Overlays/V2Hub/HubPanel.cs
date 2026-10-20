using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;
using Quaver.Shared.Graphics.Overlays.V2Hub.SongRequests;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.V2Hub;

public class HubPanel : Container
{
    private FlexContainer BackgroundLayout { get; set; }
    private SkinStoreV2Lease Skin { get; }
    private SkinV2Config RootConfig { get; }
    private bool PreviewMode { get; }
    private SkinV2HubConfig Config => RootConfig.Screens.Hub;
    private SkinV2NavigationConfig NavigationConfig => RootConfig.Shared.Navigation;

    private RoundedButton NotificationButton { get; set; }
    private RoundedButton UserButton { get; set; }
    private RoundedButton SongRequestButton { get; set; }

    public HubSection? SelectedTab { get; private set; }
    private Color SelectedSectionColor => SkinV2Color.Parse(Config.Tabs.SelectedColor);
    private Color UnselectedSectionColor => SkinV2Color.Parse(Config.Tabs.UnselectedColor);

    internal Sprite NavigationHeader { get; private set; }
    internal Sprite SectionsHeader { get; private set; }
    internal Sprite ContentBackground { get; private set; }
    internal Drawable[] TabButtons => new Drawable[] { NotificationButton, UserButton, SongRequestButton };

    public NotificationsSection NotificationsSection { get; set; }
    internal UsersSection UserSection { get; private set; }
    internal SongRequestsSection SongRequestsSection { get; private set; }

    public HubPanel(HubSection initialSection, SkinStoreV2Lease skin, SkinV2Config rootConfig,
        bool previewMode = false)
    {
        Skin = skin;
        RootConfig = rootConfig;
        PreviewMode = previewMode;
        Size = new ScalableVector2(Config.Layout.Width, WindowManager.Height);

        CreateLayout();

        CreateNavigationHeader();
        CreateSectionsHeader();
        CreateSectionContent();
        
        BackgroundLayout.RefreshLayout();
        
        SelectTab(initialSection);
    }

    private void CreateLayout()
    {
        BackgroundLayout = new FlexContainer
        {
            Parent = this,
            Size = Size,
            Direction = FlexDirection.Column,
            AlignItems = FlexAlignItems.Stretch
        };
    }

    private void CreateNavigationHeader()
    {
        var headerHeight = NavigationConfig.Button.Size + NavigationConfig.EdgePadding * 2;
        var cornerRadius = Config.Tabs.CornerRadius;
        NavigationHeader = CreateRoundedBackground(new RoundedRectCornerRadii(0, 0, 0, cornerRadius), SkinV2Color.Parse(Config.Panel.NavigationHeaderColor));
        NavigationHeader.Parent = BackgroundLayout;
        NavigationHeader.Size = new ScalableVector2(Width, headerHeight);
        BackgroundLayout.SetItemOptions(NavigationHeader, new FlexItemOptions { Basis = headerHeight, Shrink = 0 });

        var spacer = new Container
        {
            Parent = BackgroundLayout,
            Size = new ScalableVector2(0, Config.Layout.NavigationHeaderGap)
        };
        BackgroundLayout.SetItemOptions(spacer, new FlexItemOptions { Basis = Config.Layout.NavigationHeaderGap, Shrink = 0 });
    }

    private void CreateSectionsHeader()
    {
        var tabs = Config.Tabs;
        SectionsHeader = CreateRoundedBackground(new RoundedRectCornerRadii(tabs.CornerRadius, 0, tabs.CornerRadius, tabs.CornerRadius), SkinV2Color.Parse(Config.Panel.HeaderColor));
        SectionsHeader.Parent = BackgroundLayout;
        SectionsHeader.Size = new ScalableVector2(Width, tabs.HeaderHeight);
        BackgroundLayout.SetItemOptions(SectionsHeader, new FlexItemOptions { Basis = tabs.HeaderHeight, Shrink = 0 });

        var headerLayout = new FlexContainer
        {
            Parent = SectionsHeader,
            Position = new ScalableVector2(tabs.Padding, tabs.Padding),
            Size = new ScalableVector2(SectionsHeader.Width - tabs.Padding * 2, SectionsHeader.Height - tabs.Padding * 2),
            ColumnGap = tabs.Gap,
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustifyContent.Center
        };

        NotificationButton = new RoundedButton((sender, args) => SelectTab(HubSection.Notifications))
        {
            Parent = headerLayout,
            Size = new ScalableVector2(tabs.ButtonWidth, tabs.ButtonHeight),
            CornerRadius = tabs.CornerRadius,
            Tint = UnselectedSectionColor
        };
        NotificationButton.SetIcon(Skin.LoadTexture(tabs.NotificationsIcon, UserInterface.HubNotifications), new Vector2(tabs.IconSize, tabs.IconSize));
        NotificationButton.SetLabel(FontManager.GetWobbleFont(tabs.Font), LocalizationManager.Get("Screen_Hub_NotificationHeader"), tabs.FontSize, SkinV2Color.Parse(tabs.TextColor));

        UserButton = new RoundedButton((sender, args) => SelectTab(HubSection.Users))
        {
            Parent = headerLayout,
            Size = new ScalableVector2(tabs.ButtonWidth, tabs.ButtonHeight),
            CornerRadius = tabs.CornerRadius,
            Tint = UnselectedSectionColor
        };
        UserButton.SetIcon(Skin.LoadTexture(tabs.UsersIcon, UserInterface.HubOnlineUsers), new Vector2(tabs.IconSize, tabs.IconSize));
        UserButton.SetLabel(FontManager.GetWobbleFont(tabs.Font), LocalizationManager.Get("Screen_Hub_UsersHeader"), tabs.FontSize, SkinV2Color.Parse(tabs.TextColor));

        SongRequestButton = new RoundedButton((sender, args) => SelectTab(HubSection.SongRequest))
        {
            Parent = headerLayout,
            Size = new ScalableVector2(tabs.ButtonWidth, tabs.ButtonHeight),
            CornerRadius = tabs.CornerRadius,
            Tint = UnselectedSectionColor
        };
        SongRequestButton.SetIcon(Skin.LoadTexture(tabs.SongRequestsIcon, UserInterface.HubSongRequests), new Vector2(tabs.IconSize, tabs.IconSize));
        SongRequestButton.SetLabel(FontManager.GetWobbleFont(tabs.Font), LocalizationManager.Get("Screen_Hub_MapRequestHeader"), tabs.FontSize, SkinV2Color.Parse(tabs.TextColor));
    }

    private void CreateSectionContent()
    {
        ContentBackground = new Sprite
        {
            Parent = BackgroundLayout,
            Size = new ScalableVector2(Width, 0),
            Tint = SkinV2Color.Parse(Config.Panel.ContentColor),
            UpdateWhenInvisible = false
        };
        BackgroundLayout.SetItemOptions(ContentBackground, new FlexItemOptions { Basis = 0, Grow = 1, Shrink = 1 });
        BackgroundLayout.RefreshLayout();

        
        NotificationsSection = new NotificationsSection(ContentBackground.Size, Skin, RootConfig, PreviewMode)
        {
            Parent = ContentBackground,
            UpdateWhenInvisible = false
        };
        
        UserSection = new UsersSection(ContentBackground.Size, RootConfig, PreviewMode)
        {
            Parent = ContentBackground,
            UpdateWhenInvisible = false
        };
        
        SongRequestsSection = new SongRequestsSection(ContentBackground.Size, Skin, RootConfig, PreviewMode)
        {
            Parent = ContentBackground,
            UpdateWhenInvisible = false
        };
    }

    private NineSliceSprite CreateRoundedBackground(RoundedRectCornerRadii radii, Color tint)
    {
        var radius = Math.Max(0, Config.Tabs.CornerRadius);
        var margin = (int) Math.Ceiling(radius);
        var textureSize = Math.Max(20, margin * 2 + 2);

        return new NineSliceSprite(RoundedRectTextureCache.Get(textureSize, textureSize, radii), new SliceMargins(margin))
        {
            Tint = tint
        };
    }

    public void SelectTab(HubSection section)
    {
        if (SelectedTab == section)
            return;
        
        if (!PreviewMode && SelectedTab == HubSection.Notifications && NotificationsSection.CurrentFeed == NotificationsSection.NotificationFeed.Recent)
        {
            NotificationsSection.SetNewNotificationToViewed();
        }
        
        SelectedTab = section;

        if (!PreviewMode && section == HubSection.Notifications && GameBase.Game is QuaverGame notificationGame)
            notificationGame.OnlineHub?.MarkSectionAsRead(OnlineHubSectionType.Notifications);

        if (!PreviewMode && section == HubSection.SongRequest && GameBase.Game is QuaverGame game)
            game.OnlineHub?.Sections[OnlineHubSectionType.SongRequests].MarkAsRead();

        if (section != HubSection.Notifications)
        {
            NotificationsSection.Deactivate();
            ResetButtons(NotificationsSection);
        }

        if (section != HubSection.Users)
        {
            UserSection.Deactivate();
            ResetButtons(UserSection);
        }

        if (section != HubSection.SongRequest)
        {
            SongRequestsSection.Deactivate();
            ResetButtons(SongRequestsSection);
        }
        
        NotificationButton.Tint = section == HubSection.Notifications ? SelectedSectionColor : UnselectedSectionColor;
        UserButton.Tint = section == HubSection.Users ? SelectedSectionColor : UnselectedSectionColor;
        SongRequestButton.Tint = section == HubSection.SongRequest ? SelectedSectionColor : UnselectedSectionColor;
        
        NotificationsSection.Visible = section == HubSection.Notifications;
        UserSection.Visible = section == HubSection.Users;
        SongRequestsSection.Visible = section == HubSection.SongRequest;

        
    }

    internal void ResizeHeight(float height)
    {
        if (Math.Abs(Height - height) < 0.01f)
            return;

        Height = height;
        BackgroundLayout.Height = height;
        BackgroundLayout.RefreshLayout();
        NotificationsSection.ResizeHeight(ContentBackground.Height);
        UserSection.ResizeHeight(ContentBackground.Height);
        SongRequestsSection.ResizeHeight(ContentBackground.Height);
    }
    
    private static void ResetButtons(Drawable parent)
    {
        foreach (var child in parent.Children)
        {
            if (child is Button button)
                button.ResetInteractionState();

            ResetButtons(child);
        }
    }

    public enum HubSection
    {
        Notifications,
        Users,
        SongRequest,
    }
}
