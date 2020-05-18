using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tests.UI.Borders
{
    public class TestMenuBorderFooter : MenuBorder
    {
        public TestMenuBorderFooter() : base(MenuBorderType.Footer, new List<Drawable>
            {
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left), FontManager.GetWobbleFont(Fonts.LatoBlack),"Back"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_settings), FontManager.GetWobbleFont(Fonts.LatoBlack),"Options"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_open_wrench_tool_silhouette), FontManager.GetWobbleFont(Fonts.LatoBlack),"Modifiers"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol), FontManager.GetWobbleFont(Fonts.LatoBlack),"Create Playlist"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_earth_globe), FontManager.GetWobbleFont(Fonts.LatoBlack),"Online Page"),
            },
            new List<Drawable>()
            {
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_play_button), FontManager.GetWobbleFont(Fonts.LatoBlack),"Play"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_pencil), FontManager.GetWobbleFont(Fonts.LatoBlack),"Edit"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_archive_black_box), FontManager.GetWobbleFont(Fonts.LatoBlack),"Export"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_exchange_arrows), FontManager.GetWobbleFont(Fonts.LatoBlack),"Random"),
            })
        {
        }
    }
}