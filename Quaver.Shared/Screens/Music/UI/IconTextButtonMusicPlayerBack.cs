using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI
{
    public class IconTextButtonMusicPlayerBack : IconTextButton
    {
        public IconTextButtonMusicPlayerBack(MusicPlayerScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Back", (sender, args) =>
            {
                screen.ExitToMenu();
            })
        {
        }
    }
}