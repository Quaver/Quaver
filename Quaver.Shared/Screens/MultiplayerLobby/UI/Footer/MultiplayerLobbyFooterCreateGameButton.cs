using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs.Create;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Footer
{
    public class MultiplayerLobbyFooterCreateGameButton : IconTextButton
    {
        public MultiplayerLobbyFooterCreateGameButton() : base(FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol),
            FontManager.GetWobbleFont(Fonts.LatoBlack), "CREATE GAME",
            (o, e) => DialogManager.Show(new CreateGameDialog()))
        {
        }
    }
}