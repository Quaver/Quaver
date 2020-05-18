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
    public class IconTextButtonWebsite : IconTextButton
    {
        public IconTextButtonWebsite() : base(FontAwesome.Get(FontAwesomeIcon.fa_earth_globe),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Website", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://quavergame.com");
            })
        {
        }
    }
}