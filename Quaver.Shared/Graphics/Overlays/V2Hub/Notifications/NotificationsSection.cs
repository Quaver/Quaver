using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;

public class NotificationsSection : Container
{
    private FlexContainer Layout { get; set; }
    
    private RoundedButton RecentNotification { get; set; }
    private RoundedButton AllNotification { get; set; }
    private RoundedButton ClearButton { get; set; }
    
    private NotificationList Scroll { get; set; }
    
    public NotificationsSection(ScalableVector2 size)
    {
        Size = size;
        CreateLayout();
        CreateHeader();
        CreateContent();
        
        Layout.RefreshLayout();
    }
    
    private void CreateLayout()
    {
        Layout = new FlexContainer
        {
            Parent = this,
            Position = new ScalableVector2(10, 10),
            Size = new ScalableVector2(Width - 20 + 10, Height - 20), // +14 is for scroll thumb spacing
            Direction = FlexDirection.Column,
            AlignItems = FlexAlignItems.Stretch
        };
    }

    private void CreateHeader()
    {
        var header = new Sprite
        {
            Parent = Layout,
            Size = new ScalableVector2(736, 60),
            Tint = ColorHelper.FromHex("#273038")
        };
        Layout.SetItemOptions(header, new FlexItemOptions { Basis = 60, Shrink = 0 });
        
        var headerLayout = new FlexContainer
        {
            Parent = header,
            Size = new ScalableVector2(header.Width - 20, header.Height - 20),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.Center,
            JustifyContent = FlexJustifyContent.SpaceBetween
        };
        
        var buttonLayout = new FlexContainer
        {
            Parent = headerLayout,
            Size = new ScalableVector2(237 * 2, 40),
            Direction = FlexDirection.Row,
            AlignItems = FlexAlignItems.FlexStart,
            JustifyContent = FlexJustifyContent.FlexStart,
            ColumnGap = 0
        };
        
        // Connect both button together
        var cornerRadius = SkinV2BorderRadiusConfig.Normal;
        RecentNotification = new RoundedButton
        {
            Parent = buttonLayout,
            Size = new ScalableVector2(237, 40),
            CornerRadii = new RoundedRectCornerRadii(cornerRadius, 0, 0, cornerRadius), // used to remove the rounded edges on connecting sides
            Tint = ColorHelper.FromHex("#6B83B2")
        };
        RecentNotification.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_NotificationRecent", "9,999"), 18, Color.White);
        
        AllNotification = new RoundedButton
        {
            Parent = buttonLayout,
            Size = new ScalableVector2(237, 40),
            CornerRadii = new RoundedRectCornerRadii(0, cornerRadius, cornerRadius, 0),
            Tint = ColorHelper.FromHex("#181E25")
        };
        AllNotification.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_NotificationAll"), 18, Color.White);
        
        ClearButton = new RoundedButton((sender, args) => ClearAllNotifications())
        {
            Parent = headerLayout,
            Size = new ScalableVector2(124, 40),
            CornerRadius = SkinV2BorderRadiusConfig.Normal,
            Tint = ColorHelper.FromHex("#181E25")
        };
        ClearButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Hub_NotificationClearAll"), 18, Color.White);
        ClearButton.SetIcon(UserInterface.HubNotificationsClearIcon, new Vector2(20, 20));
    }
    
    private void CreateContent()
    {
        var notificationList = new List<NotificationInfo>();
        Scroll = new NotificationList(notificationList, ((int)Math.Ceiling(Height)-80)/80 +3, 0, new ScalableVector2(736, 1), new ScalableVector2(736, 1))
        {
            Parent = Layout,
            Tint = ColorHelper.FromHex("#273038"),
            EasingType = Easing.OutQuint,
            TimeToCompleteScroll = 1200,
            ScrollSpeed = 220,
            InputEnabled = true
        };
        Scroll.Scrollbar.Width = 4;
        Scroll.Scrollbar.Tint = ColorHelper.FromHex("#D9E3F4");
        Layout.SetItemOptions(Scroll, new FlexItemOptions { Basis = 0, Grow = 1 });
    }

    private void ClearAllNotifications()
    {
        
    }
}