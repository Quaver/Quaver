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
    public class IconTextButtonTwitter : IconTextButton
    {
        public IconTextButtonTwitter() : base(FontAwesome.Get(FontAwesomeIcon.fa_twitter_black_shape),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Twitter", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://twitter.com/QuaverGame");
            })
        {
        }
    }
}