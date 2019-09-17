using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Menu.Border
{
    public class MenuHeaderMain : MenuBorder
    {
        public MenuHeaderMain() : base(MenuBorderType.Header, new List<Drawable>
            {
                new MenuBorderLogo(),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_home), FontManager.GetWobbleFont(Fonts.LatoBlack),"Home"),
                new IconTextButtonDownloadMaps(),
                new IconTextButtonLeaderboards(),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_speech_bubbles_comment_option), FontManager.GetWobbleFont(Fonts.LatoBlack),"Community Chat"),
                new IconTextButtonReportBugs()
            },
            new List<Drawable>
            {
                new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_reorder_option)) { Size = new ScalableVector2(30, 30)},
                new DrawableSessionTime()
            })
        {
        }
    }
}