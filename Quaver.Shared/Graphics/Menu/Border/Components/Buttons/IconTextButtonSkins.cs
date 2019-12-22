using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonSkins : IconTextButton
    {
        public IconTextButtonSkins() : base(FontAwesome.Get(FontAwesomeIcon.fa_pencil),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Skins", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://steamcommunity.com/app/980610/workshop/");
            })
        {
        }
    }
}