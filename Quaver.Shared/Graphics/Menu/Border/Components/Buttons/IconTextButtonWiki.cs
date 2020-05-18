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
    public class IconTextButtonWiki : IconTextButton
    {
        public IconTextButtonWiki() : base(FontAwesome.Get(FontAwesomeIcon.fa_information_button),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Wiki", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://quavergame.com/wiki/");
            })
        {
        }
    }
}