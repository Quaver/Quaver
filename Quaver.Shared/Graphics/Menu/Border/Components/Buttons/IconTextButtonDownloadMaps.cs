using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonDownloadMaps : IconTextButton
    {
        public IconTextButtonDownloadMaps() : base(FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Download Maps", (sender, args) =>
            {
                if (!OnlineManager.Connected)
                {
                    NotificationManager.Show(NotificationLevel.Error, "You must be logged in to download maps!");
                    return;
                }

                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen.Exit(() => new DownloadScreen());
            })
        {
        }
    }
}