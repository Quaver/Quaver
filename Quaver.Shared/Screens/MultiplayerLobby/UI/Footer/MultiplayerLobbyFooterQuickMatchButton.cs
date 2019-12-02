using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Footer
{
    public class MultiplayerLobbyFooterQuickMatchButton : IconTextButton
    {
        /// <summary>
        /// </summary>
        public static Random RNG { get; } = new Random();

        /// <summary>
        /// </summary>
        public MultiplayerLobbyFooterQuickMatchButton() : base(FontAwesome.Get(FontAwesomeIcon.fa_lightning_bolt_shadow),
            FontManager.GetWobbleFont(Fonts.LatoBlack), "Quick Match", (o, e) =>
            {
                var openGames = OnlineManager.MultiplayerGames.Values.ToList()
                    .FindAll(x => !x.HasPassword && x.PlayerIds.Count < x.MaxPlayers);

                if (openGames.Count == 0)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "There are no available open games to join.");
                    return;
                }

                DialogManager.Show(new JoinGameDialog(openGames[RNG.Next(0, openGames.Count - 1)]));
            })
        {
        }
    }
}