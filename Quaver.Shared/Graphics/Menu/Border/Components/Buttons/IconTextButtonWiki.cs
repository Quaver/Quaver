using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonWiki : IconTextButton
    {
        public IconTextButtonWiki() : base(FontAwesome.Get(FontAwesomeIcon.fa_information_button),
            FontManager.GetWobbleFont(Fonts.InterBold),"Wiki", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://quavergame.com/wiki/");
            }, localizationKey: "Screen_Main_Menu_Wiki")
        {
        }
    }
}
