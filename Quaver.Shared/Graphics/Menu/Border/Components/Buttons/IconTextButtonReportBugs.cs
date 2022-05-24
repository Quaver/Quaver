using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
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