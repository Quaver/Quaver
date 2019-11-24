using System.Collections.Generic;
using Quaver.Shared.Graphics.Menu.Border;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Theater.UI.Footer
{
    public class TheaterFooter : MenuBorder
    {
        public TheaterFooter(TheaterScreen screen) : base(MenuBorderType.Footer, new List<Drawable>()
        {
            new TheaterFooterBackButton(screen)
        }, new List<Drawable>()
        {
            new TheaterFooterPlayButton(screen)
        })
        {
        }
    }
}