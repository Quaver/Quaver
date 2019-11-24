using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Tournament;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Theater.UI.Footer
{
    public class TheaterFooterPlayButton : IconTextButton
    {
        public TheaterFooterPlayButton(TheaterScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_play_sign),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Play", (sender, args) =>
            {
                if (screen.Replays.Count < 2)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "You must load at least 2 replays in order to watch.");
                    return;
                }

                screen.Exit(() => new TournamentScreen(screen.Replays));
            })
        {
        }
    }
}