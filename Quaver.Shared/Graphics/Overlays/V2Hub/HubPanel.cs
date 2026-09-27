using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;
using Quaver.Shared.Graphics.Overlays.V2Hub.SongRequests;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.V2Hub;

public class HubPanel : Container
{
    private FlexContainer BackgroundLayout { get; set; }
    private SkinStoreV2Lease Skin { get; }
    private SkinV2NavigationConfig NavigationConfig => Skin.Config.Shared.Navigation;
    private HeaderScreenNavigation HeaderControls { get; set; }

    public RoundedButton ProfileButton => HeaderControls.ProfileButton;
    public float ProfileDropdownGap => NavigationConfig.Profile.DropdownGap;

    private RoundedButton NotificationButton { get; set; }
    private RoundedButton UserButton { get; set; }
    private RoundedButton SongRequestButton { get; set; }
    
    public HubSection? SelectedTab { get; private set; }
    private Color SelectedSectionColor { get; set; } = ColorHelper.FromHex("#6B83B2");
    private Color UnselectedSectionColor { get; set; } = ColorHelper.FromHex("#273038");

    private Sprite ContentBackground { get; set; }
    
    public NotificationsSection NotificationsSection { get; set; }
    private UsersSection UserSection { get; set; }
    private SongRequestsSection SongRequestsSection { get; set; }

    public HubPanel(HubSection initialSection, Action closeHub, Action openProfile)
    {
        Skin = SkinManager.AcquireV2();
        Size = new ScalableVector2(736, WindowManager.Height);
        
        CreateLayout();

        CreatePlayerHeader(closeHub, openProfile);
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

    private void CreatePlayerHeader(Action closeHub, Action openProfile)
    {
        var headerHeight = NavigationConfig.Button.Size + NavigationConfig.EdgePadding * 2;
        var header = new Sprite
        {
            Parent = BackgroundLayout,
            Size = new ScalableVector2(Width, headerHeight),
            Tint = ColorHelper.FromHex("#181E25")
        };
        BackgroundLayout.SetItemOptions(header, new FlexItemOptions { Basis = headerHeight, Shrink = 0 });

        HeaderControls = new HeaderScreenNavigation(NavigationConfig);
        HeaderControls.ProfileButton.Clicked += (sender, args) => openProfile();

        var headerLayout = new FlexContainer
        {
            Parent = header,
            Alignment = Alignment.MidRight,
            X = -NavigationConfig.EdgePadding,
            Size = new ScalableVector2(header.Width - NavigationConfig.EdgePadding * 2, NavigationConfig.Button.Size),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            ColumnGap = NavigationConfig.ItemSpacing
        };
        HeaderControls.Parent = headerLayout;
        headerLayout.SetItemOptions(HeaderControls, new FlexItemOptions { Basis = HeaderControls.Width, Grow = 1, Shrink = 0 });

        var closeButton = new RoundedButton((sender, args) => closeHub())
        {
            Parent = headerLayout,
            Size = new ScalableVector2(NavigationConfig.Button.Size, NavigationConfig.Button.Size),
            CornerRadius = NavigationConfig.Button.CornerRadius,
            Tint = SkinV2Color.Parse(NavigationConfig.Button.BackgroundColor)
        };
        closeButton.SetIcon(GlobalIcons.Get(GlobalIcon.Burger), new Vector2(NavigationConfig.Button.IconSize, NavigationConfig.Button.IconSize));
        closeButton.Icon.Tint = SkinV2Color.Parse(NavigationConfig.Button.ForegroundColor);
        headerLayout.SetItemOptions(closeButton, new FlexItemOptions { Basis = closeButton.Width, Shrink = 0 });

        headerLayout.RefreshLayout();
        HeaderControls.RefreshLayout();
        
        var spacer = new Container
        {
            Parent = BackgroundLayout,
            Size = new ScalableVector2(0, 10)
        };
        BackgroundLayout.SetItemOptions(spacer, new FlexItemOptions { Basis = 10, Shrink = 0 });
    }

    private void CreateSectionsHeader()
    {
        var header = new Sprite
        {
            Parent = BackgroundLayout,
            Size = new ScalableVector2(736, 60),
            Tint = ColorHelper.FromHex("#181E25")
        };
        BackgroundLayout.SetItemOptions(header, new FlexItemOptions { Basis = 60, Shrink = 0 });
        
        var headerLayout = new FlexContainer
        {
            Parent = header,
            Position = new ScalableVector2(10, 10),
            Size = new ScalableVector2(header.Width - 20, header.Height - 20),
            ColumnGap = 10,
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustifyContent.Center
        };
        
        NotificationButton = new RoundedButton((sender, args) => SelectTab(HubSection.Notifications))
        {
            Parent = headerLayout,
            Size = new ScalableVector2(232, 40),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = UnselectedSectionColor
        };
        NotificationButton.SetIcon(UserInterface.HubNotifications, new Vector2(20, 20));
        NotificationButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_NotificationHeader"), 18, Color.White);
        
        UserButton = new RoundedButton((sender, args) => SelectTab(HubSection.Users))
        {
            Parent = headerLayout,
            Size = new ScalableVector2(232, 40),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = UnselectedSectionColor
        };
        UserButton.SetIcon(UserInterface.HubOnlineUsers, new Vector2(20, 20));
        UserButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_UsersHeader"), 18, Color.White);
        
        SongRequestButton = new RoundedButton((sender, args) => SelectTab(HubSection.SongRequest))
        {
            Parent = headerLayout,
            Size = new ScalableVector2(232, 40),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = UnselectedSectionColor
        };
        SongRequestButton.SetIcon(UserInterface.HubSongRequests, new Vector2(20, 20));
        SongRequestButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_MapRequestHeader"), 18, Color.White);
    }

    private void CreateSectionContent()
    {
        ContentBackground = new Sprite
        {
            Parent = BackgroundLayout,
            Size = new ScalableVector2(736, 0),
            Tint = ColorHelper.FromHex("#273038"),
            UpdateWhenInvisible = false
        };
        BackgroundLayout.SetItemOptions(ContentBackground, new FlexItemOptions { Basis = 0, Grow = 1, Shrink = 1 });
        BackgroundLayout.RefreshLayout();

        
        NotificationsSection = new NotificationsSection(ContentBackground.Size)
        {
            Parent = ContentBackground,
            UpdateWhenInvisible = false
        };
        
        UserSection = new UsersSection(ContentBackground.Size)
        {
            Parent = ContentBackground,
            UpdateWhenInvisible = false
        };
        
        SongRequestsSection = new SongRequestsSection(ContentBackground.Size)
        {
            Parent = ContentBackground,
            UpdateWhenInvisible = false
        };
    }

    public void SelectTab(HubSection section)
    {
        if (SelectedTab == section)
            return;
        
        if (SelectedTab == HubSection.Notifications && NotificationsSection.CurrentFeed == NotificationsSection.NotificationFeed.Recent)
        {
            NotificationsSection.SetNewNotificationToViewed();
        }
        
        SelectedTab = section;

        if (section == HubSection.SongRequest && GameBase.Game is QuaverGame game)
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
    
    private static void ResetButtons(Drawable parent)
    {
        foreach (var child in parent.Children)
        {
            if (child is Button button)
                button.ResetInteractionState();

            ResetButtons(child);
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        Skin.Dispose();
    }

    public enum HubSection
    {
        Notifications,
        Users,
        SongRequest,
    }
}
