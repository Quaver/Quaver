using System.Collections.Generic;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Borders.Footer
{
    public class SelectMenuFooter : MenuBorder
    {
        public SelectMenuFooter(SelectionScreen screen) : base(MenuBorderType.Footer, new List<Drawable>
        {
            new IconTextButtonSelectBack(screen),
            new IconTextButtonOptions(),
            new IconTextButtonModifiers(screen.ActiveLeftPanel),
            new IconTextButtonMapPreview(screen.ActiveLeftPanel),
            new IconTextButtonProfile(screen.ActiveLeftPanel),
        }, new List<Drawable>
        {
            new IconTextButtonPlay(screen),
            new IconTextButtonEdit(screen),
            new IconTextButtonRandom(screen),
            new IconTextButtonCreatePlaylist()
        })
        {
        }
    }
}