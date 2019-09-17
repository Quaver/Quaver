using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonLeaderboards: IconTextButton
    {
        public IconTextButtonLeaderboards() : base(FontAwesome.Get(FontAwesomeIcon.fa_trophy),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Leaderboards", (sender, args) =>
            {
                var mode = ConfigManager.SelectedGameMode?.Value ?? GameMode.Keys4;
                BrowserHelper.OpenURL($"https://quavergame.com/leaderboard/?mode={(int) mode}");
            })
        {
        }
    }
}