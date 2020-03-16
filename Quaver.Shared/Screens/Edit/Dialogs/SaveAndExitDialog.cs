using System;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class SaveAndExitDialog : YesNoDialog
    {
        private IconButton YellowNoButton { get; }

        public SaveAndExitDialog(EditScreen screen) : base("EXIT EDITOR",
            "You have unsaved changes. Would you like to save?")
        {
            YesAction += () =>
            {
                screen.Save(true);
                screen.ExitToSongSelect();
            };

            const float scale = 0.90f;

            YesButton.Size = new ScalableVector2(YesButton.Width * scale, YesButton.Height * scale);
            NoButton.Size = new ScalableVector2(NoButton.Width * scale, NoButton.Height * scale);

            YellowNoButton = new IconButton(UserInterface.NoYellowButton, (sender, args) =>
            {
                screen.ExitToSongSelect();
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