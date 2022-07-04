using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
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