using System.Collections.Generic;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Main.UI
{
    public class MainMenuFooter : MenuBorder
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MainMenuFooter() : base(MenuBorderType.Footer, new List<Drawable>()
        {
            new MenuFooterButtonQuit(),
            new IconTextButtonOptions(),
        }, new List<Drawable>()
        {

        })
        {
        }
    }
}