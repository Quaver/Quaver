using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Overlays.V2Hub.Users;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.V2Hub;

public class HubPanel : Container
{
    private FlexContainer BackgroundLayout { get; set; }
    
    private RoundedButton NotificationButton { get; set; }
    private RoundedButton UserButton { get; set; }
    private RoundedButton SongRequestButton { get; set; }
    
    private HubSection SelectedTab { get; set; }
    private Color SelectedSectionColor { get; set; } = ColorHelper.FromHex("#6B83B2");
    private Color UnselectedSectionColor { get; set; } = ColorHelper.FromHex("#273038");

    private Sprite ContentBackground { get; set; }
    
    private Sprite NotificationsContent { get; set; }
    private UsersSection UserSection { get; set; }
    private Sprite SongRequestContent { get; set; }

    public HubPanel()
    {
        Size = new ScalableVector2(736, WindowManager.Height);
        
        CreateLayout();
        CreatePlayerHeader();
        CreateSectionsHeader();
        CreateSectionContent();
        
        BackgroundLayout.RefreshLayout();
        
        SelectTab(HubSection.Users);
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

    private void CreatePlayerHeader()
    {
        var header = new Sprite
        {
            Parent = BackgroundLayout,
            Size = new ScalableVector2(736, 70),
            Tint = ColorHelper.FromHex("#181E25")
        };
        BackgroundLayout.SetItemOptions(header, new FlexItemOptions { Basis = 70, Shrink = 0 });
        
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
            Tint = ColorHelper.FromHex("#273038")
        };
        BackgroundLayout.SetItemOptions(ContentBackground, new FlexItemOptions { Basis = 0, Grow = 1, Shrink = 1 });
        BackgroundLayout.RefreshLayout();

        
        NotificationsContent = new Sprite
        {
            Parent = ContentBackground,
            Size = ContentBackground.Size,
            Tint = ColorHelper.FromHex("#673038")
        };
        
        UserSection = new UsersSection(ContentBackground.Size)
        {
            Parent = ContentBackground,
        };
        
        SongRequestContent = new Sprite
        {
            Parent = ContentBackground,
            Size = ContentBackground.Size,
            Tint = ColorHelper.FromHex("#473038")
        };
    }

    private void SelectTab(HubSection section)
    {
        SelectedTab = section;

        if (section != HubSection.Users)
            UserSection.Deactivate();
        
        NotificationButton.Tint = section == HubSection.Notifications ? SelectedSectionColor : UnselectedSectionColor;
        UserButton.Tint = section == HubSection.Users ? SelectedSectionColor : UnselectedSectionColor;
        SongRequestButton.Tint = section == HubSection.SongRequest ? SelectedSectionColor : UnselectedSectionColor;
        
        NotificationsContent.Visible = section == HubSection.Notifications;
        UserSection.Visible = section == HubSection.Users;
        SongRequestContent.Visible = section == HubSection.SongRequest;
    }

    private enum HubSection
    {
        Notifications,
        Users,
        SongRequest,
    }
}
