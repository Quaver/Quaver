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
    public class IconTextButtonReportBugs: IconTextButton
    {
        public IconTextButtonReportBugs() : base(FontAwesome.Get(FontAwesomeIcon.fa_bug),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Report Bugs", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://github.com/Quaver/Quaver/issues");
            })
        {
        }
    }
}