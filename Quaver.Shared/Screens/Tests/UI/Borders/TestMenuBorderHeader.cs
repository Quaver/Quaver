using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tests.UI.Borders
{
    public class TestMenuBorderHeader : MenuBorder
    {
        public TestMenuBorderHeader() : base(MenuBorderType.Header, new List<Drawable>
            {
                new MenuBorderLogo(),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_home), FontManager.GetWobbleFont(Fonts.LatoBlack),"Home"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive), FontManager.GetWobbleFont(Fonts.LatoBlack),"Download"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_speech_bubbles_comment_option), FontManager.GetWobbleFont(Fonts.LatoBlack),"Community Chat"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_bug), FontManager.GetWobbleFont(Fonts.LatoBlack),"Report Bugs"),
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