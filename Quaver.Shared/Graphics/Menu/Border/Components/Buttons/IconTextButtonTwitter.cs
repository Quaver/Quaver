using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
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