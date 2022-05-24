using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonDiscord : IconTextButton
    {
        public IconTextButtonDiscord() : base(FontAwesome.Get(FontAwesomeIcon.fa_discord),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Discord", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://discord.gg/quaver", true);
            })
        {
        }
    }
}