using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Menu.Border.Components.Users;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Tests.UI.Borders
{
    public class TestMenuBorderHeader : MenuBorder
    {
        private DrawableLoggedInUser LoggedInUser { get; } = new DrawableLoggedInUser();

        public TestMenuBorderHeader() : base(MenuBorderType.Header, new List<Drawable>
            {
                new MenuBorderLogo(),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_home), FontManager.GetWobbleFont(Fonts.LatoBlack),"Home"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive), FontManager.GetWobbleFont(Fonts.LatoBlack),"Download Maps"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_trophy), FontManager.GetWobbleFont(Fonts.LatoBlack),"Leaderboards"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_speech_bubbles_comment_option), FontManager.GetWobbleFont(Fonts.LatoBlack),"Community Chat"),
                new IconTextButton(FontAwesome.Get(FontAwesomeIcon.fa_bug), FontManager.GetWobbleFont(Fonts.LatoBlack),"Report Bugs"),
            },
            new List<Drawable>
            {
                new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_reorder_option)) { Size = new ScalableVector2(26, 26)},
            })
        {
            RightAlignedItems.Add(LoggedInUser);
            RightAlignedItems.Add(new DrawableSessionTime());
            AlignRightItems();

        }
    }
}