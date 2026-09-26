using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.V2Hub.Notifications;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
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
    
    /// <summary>
    /// </summary>
    private bool IsClosing { get; set; }
    
    public OverlayDialog() : base(0)
    {
        CreateContent();
    }


    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="gameTime"></param>
    public override void HandleInput(GameTime gameTime)
    {
        if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            Close();

        if (MouseManager.IsUniqueClick(MouseButton.Left) && !Hub.IsHovered())
            Close();
    }
    
    public override void CreateContent()
    {
        Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
        Container.Size = Size;

        Hub = new HubPanel()
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            X = 736
        };
        Hub.MoveToX(0, Easing.OutCubic, 200);
    }
    
    /// <summary>
    /// </summary>
    public void Close()
    {
        if(IsClosing) return;
        
        IsClosing = true;

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
    
    /// <summary>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMultiplayerGameStarted(object sender, GameStartedEventArgs e) => Close();
}
