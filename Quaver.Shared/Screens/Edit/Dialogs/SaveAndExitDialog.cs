using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Buttons;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class SaveAndExitDialog : YesNoDialog
    {
        private RoundedButton YellowNoButton { get; }

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

            YellowNoButton = new RoundedButton((sender, args) =>
            {
                screen.ExitToSongSelect();
                Close();
            })
            {
                Parent = Panel,
                Size = YesButton.Size,
                Alignment = Alignment.BotCenter,
                Y = YesButton.Y,
                Tint = ColorHelper.HexToColor("#F2994A")
            };

            YellowNoButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "NO", 20, Color.White);

            YesButton.X -= 80;
            NoButton.X = -YesButton.X;
        }

        public override void Close()
        {
            YellowNoButton.IsClickable = false;
            YellowNoButton.PerformHoverFade = false;
            base.Close();
        }
    }
}