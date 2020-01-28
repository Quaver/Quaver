using System.Collections.Generic;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Screens.Downloading.UI.Footer.Items;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Downloading.UI.Footer
{
    public class DownloadingFooter : MenuBorder
    {
        public DownloadingFooter() : base(MenuBorderType.Footer, new List<Drawable>
        {
            new DownloadingFooterBackButton(),
            new IconTextButtonOptions()
        }, new List<Drawable>
        {
        })
        {
        }
    }
}