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
    public class IconTextButtonDiscord : IconTextButton
    {
        public IconTextButtonDiscord() : base(FontAwesome.Get(FontAwesomeIcon.fa_discord),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Discord", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://discord.gg/nJa8VFr", true);
            })
        {
        }
    }
}