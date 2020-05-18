using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.MultiplayerLobby;
using Wobble.Bindables;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Footer
{
    public class IconTextButtonMultiplayerMatchHistory : IconTextButton
    {
        public IconTextButtonMultiplayerMatchHistory(Bindable<MultiplayerGame> game) : base(FontAwesome.Get(FontAwesomeIcon.fa_time),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"History", (sender, args) =>
            {
                BrowserHelper.OpenURL($"https://quavergame.com/multiplayer/game/{game.Value.GameId}");
            })
        {
        }
    }
}