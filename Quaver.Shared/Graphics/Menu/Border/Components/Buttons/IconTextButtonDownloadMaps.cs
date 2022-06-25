using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Downloading;
using Wobble;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonDownloadMaps : MenuBorderScreenChangeButton
    {
        public override QuaverScreenType Screen { get; } = QuaverScreenType.Download;

        public IconTextButtonDownloadMaps() : base(FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive),
            FontManager.GetWobbleFont(Fonts.LatoBlack), "Maps")
        {
        }

        public override void OnClick()
        {
            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen.Type == QuaverScreenType.Multiplayer)
            {
                NotificationManager.Show(NotificationLevel.Warning, $"You can only download maps in multiplayer while you are host during song select!");
                return;
            }

            game.CurrentScreen.Exit(() => new DownloadingScreen(game.CurrentScreen.Type));
        }
    }
}