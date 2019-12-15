using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Selection;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Footer
{
    public class IconTextButtonMultiplayerSelectMap : IconTextButton
    {
        public IconTextButtonMultiplayerSelectMap(QuaverScreen screen) : base(FontAwesome.Get(FontAwesomeIcon.fa_music_note_black_symbol),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"Select Map", (sender, args) =>
            {
                var multi = (MultiplayerGameScreen) screen;
                multi.DontLeaveGameUponScreenSwitch = true;

                screen.Exit(() => new SelectionScreen());
            })
        {
        }
    }
}