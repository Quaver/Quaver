using System.Collections.Generic;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Screens.Theater.UI.Footer;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Footer
{
    public class MultiplayerLobbyFooter : MenuBorder
    {
        public MultiplayerLobbyFooter(QuaverScreen screen) : base(MenuBorderType.Footer, new List<Drawable>()
        {
            new FooterBackButton(screen),
            new IconTextButtonOptions()
        }, new List<Drawable>()
        {
            new MultiplayerLobbyFooterQuickMatchButton(),
            new MultiplayerLobbyFooterCreateGameButton()
        })
        {
        }
    }
}