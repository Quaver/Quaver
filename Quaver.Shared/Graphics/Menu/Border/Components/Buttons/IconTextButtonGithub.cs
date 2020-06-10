using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Options;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonGithub : IconTextButton
    {
        public IconTextButtonGithub() : base(FontAwesome.Get(FontAwesomeIcon.fa_github_logo),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"GitHub", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://github.com/Quaver");
            })
        {
        }
    }
}