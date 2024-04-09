using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class UnsavedChangesSwitchMapDialog : YesNoDialog
    {
        private IconButton YellowNoButton { get; }
        public UnsavedChangesSwitchMapDialog(EditScreen screen, Map map) : base("SAVE CHANGES",
            "You have unsaved changes. Would you like to save\n" +
            "before switching to another difficulty?")
        {
            YesAction += () =>
            {
                screen.Save(true);
                NotificationManager.Show(NotificationLevel.Success, "Your map has been successfully saved!");

                screen.SwitchToMap(map);
            };

            const float scale = 0.90f;

            YesButton.Size = new ScalableVector2(YesButton.Width * scale, YesButton.Height * scale);
            NoButton.Size = new ScalableVector2(NoButton.Width * scale, NoButton.Height * scale);

            YellowNoButton = new IconButton(UserInterface.NoYellowButton, (sender, args) =>
            {
                screen.SwitchToMap(map, true);
                Close();
            })
            {
                Parent = Panel,
                Size = YesButton.Size,
                Alignment = Alignment.BotCenter,
                Y = YesButton.Y
            };

            YesButton.X -= 80;
            NoButton.X = -YesButton.X;
        }
        
        public override void Close()
        {
            YellowNoButton.IsClickable = false;
            YellowNoButton.IsPerformingFadeAnimations = false;
            base.Close();
        }
    }
}