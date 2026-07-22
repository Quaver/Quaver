using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Wobble.Graphics.Buttons;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class UnsavedChangesSwitchMapDialog : YesNoDialog
    {
        private RoundedButton YellowNoButton { get; }
        public UnsavedChangesSwitchMapDialog(EditScreen screen, Map map) : base(
            LocalizationManager.Get("Screen_Editor_SaveChanges"),
            LocalizationManager.Get("Screen_Editor_SaveBeforeSwitchingDifficulty"))
        {
            YesAction += () =>
            {
                screen.Save(true);
                NotificationManager.Show(NotificationLevel.Success,
                    LocalizationManager.Get("Screen_Editor_MapSavedSuccessfully"));

                screen.SwitchToMap(map);
            };

            const float scale = 0.90f;

            YesButton.Size = new ScalableVector2(YesButton.Width * scale, YesButton.Height * scale);
            NoButton.Size = new ScalableVector2(NoButton.Width * scale, NoButton.Height * scale);

            YellowNoButton = new RoundedButton((sender, args) =>
            {
                screen.SwitchToMap(map, true);
                Close();
            })
            {
                Parent = Panel,
                Size = YesButton.Size,
                Alignment = Alignment.BotCenter,
                Y = YesButton.Y,
                Tint = ColorHelper.HexToColor("#F2994A")
            };

            YellowNoButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold),
                LocalizationManager.Get("Screen_Editor_No"), 20, Color.White);

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
