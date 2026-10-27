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
    private LoggedInUserDropdown ProfileDropdown { get; set; }

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

        if (ProfileDropdown != null)
        {
            if (!Hub.ProfileButton.IsHovered && !ProfileDropdown.IsHovered())
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

        Hub = new HubPanel(InitialSection, Close, ToggleProfileDropdown)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            X = 736
        };
        Hub.MoveToX(0, Easing.OutCubic, 200);
    }

    private void ToggleProfileDropdown()
    {
        if (ProfileDropdown != null)
        {
            DismissProfileDropdown();
            return;
        }

        var profile = Hub.ProfileButton;
        ProfileDropdown = new LoggedInUserDropdown(Container)
        {
            Parent = Container,
            Position = new ScalableVector2(
                profile.AbsolutePosition.X + profile.AbsoluteSize.X - LoggedInUserDropdown.ContainerSize.X.Value - Container.AbsolutePosition.X,
                profile.AbsolutePosition.Y + profile.AbsoluteSize.Y + Hub.ProfileDropdownGap - Container.AbsolutePosition.Y)
        };
    }

    private void DismissProfileDropdown()
    {
        ProfileDropdown?.Destroy();
        ProfileDropdown = null;
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
        DismissProfileDropdown();
        base.Destroy();
    }
    
    /// <summary>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMultiplayerGameStarted(object sender, GameStartedEventArgs e) => Close();
}
