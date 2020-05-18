using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.MultiplayerLobby;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Footer
{
    public class IconTextButtonLeaveMultiplayerGame : IconTextButton
    {
        public IconTextButtonLeaveMultiplayerGame(QuaverScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_chevron_pointing_to_the_left),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Leave", (sender, args) =>
            {
                screen.Exit(() => new MultiplayerLobbyScreen());
            })
        {
        }
    }
}