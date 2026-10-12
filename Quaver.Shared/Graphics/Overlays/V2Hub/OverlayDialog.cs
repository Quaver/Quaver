using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.V2.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.V2Hub;

public class OverlayDialog : DialogScreen
{
    /// <summary>
    /// </summary>
    private HubPanel Hub { get; set; }
    private FlexContainer NavigationHeaderLayout { get; set; }
    private LoggedInUserDropdown ProfileDropdown { get; set; }
    private Drawable ProfileDropdownTrigger { get; set; }

    private HubPanel.HubSection InitialSection { get; }

    public HubPanel.HubSection? SelectedTab => Hub?.SelectedTab;
    
    /// <summary>
    /// </summary>
    private bool IsClosing { get; set; }
    
    public OverlayDialog(HubPanel.HubSection initialSection = HubPanel.HubSection.Users) : base(0)
    {
        InitialSection = initialSection;
        CreateContent();
    }

    public void SelectTab(HubPanel.HubSection section) => Hub.SelectTab(section);


    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="gameTime"></param>
    public override void HandleInput(GameTime gameTime)
    {
        if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
        {
            if (ProfileDropdown != null)
                DismissProfileDropdown();
            else
                Close();
            return;
        }

        if (!MouseManager.IsUniqueClick(MouseButton.Left))
            return;

        if (NotificationManager.IsPointerOverActiveNotification)
            return;

        if (ProfileDropdown != null)
        {
            if (!ProfileDropdownTrigger.IsHovered() && !ProfileDropdown.IsHovered())
                DismissProfileDropdown();
            return;
        }

        if (!Hub.IsHovered())
            Close();
    }
    
    public override void CreateContent()
    {
        Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
        Container.Size = Size;

        Hub = new HubPanel(InitialSection)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            X = 736
        };

        NavigationHeaderLayout = new FlexContainer
        {
            Parent = Container
        };
        ScreenNavigation.SetHubOverlayState(true, NavigationHeaderLayout);
        Hub.MoveToX(0, Easing.OutCubic, 200);
    }

    internal void ToggleProfileDropdown(Drawable trigger, float dropdownGap)
    {
        if (ProfileDropdown != null)
        {
            DismissProfileDropdown();
            return;
        }

        ProfileDropdownTrigger = trigger;
        ProfileDropdown = new LoggedInUserDropdown(Container)
        {
            Parent = Container,
            Position = new ScalableVector2(
                trigger.AbsolutePosition.X + trigger.AbsoluteSize.X - LoggedInUserDropdown.ContainerSize.X.Value - Container.AbsolutePosition.X,
                trigger.AbsolutePosition.Y + trigger.AbsoluteSize.Y + dropdownGap - Container.AbsolutePosition.Y)
        };
    }

    private void DismissProfileDropdown()
    {
        ProfileDropdown?.Destroy();
        ProfileDropdown = null;
        ProfileDropdownTrigger = null;
    }
    
    /// <summary>
    /// </summary>
    public void Close()
    {
        if(IsClosing) return;
        
        IsClosing = true;
        DismissProfileDropdown();

        ClearAnimations();
        Hub.MoveToX(736, Easing.InCubic, 200);

        if (GameBase.Game is QuaverGame game && game.CurrentScreen?.Type == QuaverScreenType.Gameplay)
        {
            if (game.CurrentScreen is GameplayScreen gameplay)
                game.GlobalUserInterface.Cursor.Alpha = gameplay.InReplayMode && gameplay.SpectatorClient == null ? 1 : 0;
        }

        if (Hub.SelectedTab == HubPanel.HubSection.Notifications && Hub.NotificationsSection.CurrentFeed == NotificationsSection.NotificationFeed.Recent)
        {
            Hub.NotificationsSection.SetNewNotificationToViewed();
        }
        ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 300);
    }

    public override void Destroy()
    {
        ScreenNavigation.SetHubOverlayState(false);
        DismissProfileDropdown();
        base.Destroy();
    }
    
    /// <summary>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMultiplayerGameStarted(object sender, GameStartedEventArgs e) => Close();
}
