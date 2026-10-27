using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.V2Chatting;
using Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
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
    private SkinStoreV2Lease Skin { get; }
    private SkinV2Config RootConfig { get; set; }
    private SkinV2HubConfig Config => RootConfig.Screens.Hub;

    private HubPanel.HubSection InitialSection { get; }

    public HubPanel.HubSection? SelectedTab => Hub?.SelectedTab;
    
    /// <summary>
    /// </summary>
    private bool IsClosing { get; set; }
    
    public OverlayDialog(HubPanel.HubSection initialSection = HubPanel.HubSection.Users) : base(0)
    {
        var chat = DialogManager.Dialogs.OfType<ChatV2Dialog>().LastOrDefault();
        if (chat != null)
            DialogManager.Dismiss(chat);

        InitialSection = initialSection;
        Skin = SkinManager.AcquireV2();
        RootConfig = Skin.Config;
        CreateContent();
        FadeTo(0.75f, Easing.Linear, Config.Layout.AnimationDurationMilliseconds);
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

        if (!MouseManager.IsUniquePress(MouseButton.Left))
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

        BuildHub(true);
        NavigationHeaderLayout = new FlexContainer
        {
            Parent = Container
        };
        ScreenNavigation.SetHubOverlayState(true, NavigationHeaderLayout);
    }

    private void BuildHub(bool animate)
    {
        var selectedSection = Hub?.SelectedTab ?? InitialSection;
        Hub?.Destroy();
        DismissProfileDropdown();

        Hub = new HubPanel(selectedSection, Skin, RootConfig)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            X = animate ? Config.Layout.Width : 0
        };

        if (animate)
            Hub.MoveToX(0, Easing.OutCubic, Config.Layout.AnimationDurationMilliseconds);

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
        FadeTo(0, Easing.Linear, Config.Layout.AnimationDurationMilliseconds);
        Hub.MoveToX(Config.Layout.Width, Easing.InCubic, Config.Layout.AnimationDurationMilliseconds);

        if (GameBase.Game is QuaverGame game && game.CurrentScreen?.Type == QuaverScreenType.Gameplay)
        {
            if (game.CurrentScreen is GameplayScreen gameplay)
                game.GlobalUserInterface.Cursor.Alpha = gameplay.InReplayMode && gameplay.SpectatorClient == null ? 1 : 0;
        }

        if (Hub.SelectedTab == HubPanel.HubSection.Notifications && Hub.NotificationsSection.CurrentFeed == NotificationsSection.NotificationFeed.Recent)
        {
            Hub.NotificationsSection.SetNewNotificationToViewed();
        }
        ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this),
            Config.Layout.AnimationDurationMilliseconds + 100);
    }

    public override void Update(GameTime gameTime)
    {
        var width = WindowManager.Width;
        var height = WindowManager.Height;
        if (Container.Width != width || Container.Height != height)
        {
            Size = new ScalableVector2(width, height);
            Container.Size = Size;
            Hub.ResizeHeight(height);
            NavigationHeaderLayout.Size = new ScalableVector2(width - RootConfig.Shared.Navigation.EdgePadding, RootConfig.Shared.Navigation.Button.Size);
            NavigationHeaderLayout.RefreshLayout();
        }

        base.Update(gameTime);
    }

    public override void Destroy()
    {
        ScreenNavigation.SetHubOverlayState(false);
        DismissProfileDropdown();
        base.Destroy();
        Skin.Dispose();
    }
    
    /// <summary>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMultiplayerGameStarted(object sender, GameStartedEventArgs e) => Close();
}
