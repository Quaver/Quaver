using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Screens.Main;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Theater.UI.Footer
{
    public class FooterBackButton : IconTextButton
    {
        public FooterBackButton(QuaverScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Back", (sender, args) =>
            {
                screen.Exit(() => new MainMenuScreen());
            })
        {
        }
    }
}