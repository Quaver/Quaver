using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border.Components.Buttons
{
    public class IconTextButtonClans : IconTextButton
    {
        public IconTextButtonClans() : base(FontAwesome.Get(FontAwesomeIcon.fa_group_profile_users),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Clans", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://two.quavergame.com/leaderboard/clans");
            })
        {
        }
    }
}