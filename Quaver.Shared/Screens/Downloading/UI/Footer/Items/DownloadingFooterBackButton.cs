using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Screens.MultiplayerLobby;
using Wobble;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Footer.Items
{
    public class DownloadingFooterBackButton : IconTextButton
    {
        public DownloadingFooterBackButton() : base(FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Back", (sender, args) =>
            {
                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as DownloadingScreen;
                screen?.ExitToPreviousScreen();
            })
        {
        }
    }
}