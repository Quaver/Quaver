using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Games
{
    public class MultiplayerGameRightClickOptions : RightClickOptions
    {
        private const string JoinGame = "Join Game";

        private const string ViewMatchHistory = "View Match History";

        private const string SpectateGame = "Spectate Game";

        public MultiplayerGameRightClickOptions(MultiplayerGame game) : base(GetOptions(game), new ScalableVector2(200, 40), 22)
        {
            ItemSelected += (sender, args) =>
            {
                switch (args.Text)
                {
                    case JoinGame:
                        if (game.HasPassword)
                            DialogManager.Show(new JoinPasswordGameDialog(game));
                        else
                            DialogManager.Show(new JoinGameDialog(game));
                        break;
                    case ViewMatchHistory:
                        BrowserHelper.OpenURL($"https://quavergame.com/multiplayer/game/{game.GameId}");
                        break;
                    case SpectateGame:
                        var isContributor = OnlineManager.Self.OnlineUser.UserGroups.HasFlag(UserGroups.Contributor);
                        if (!(OnlineManager.IsDonator || isContributor))
                        {
                            NotificationManager.Show(NotificationLevel.Warning, "You must be a donator in order to spectate multiplayer games!");
                            return;
                        }

                        if (game.HasPassword)
                            DialogManager.Show(new JoinPasswordGameDialog(game, true));
                        else
                            DialogManager.Show(new JoinGameDialog(game, null, false, true));
                        break;
                }
            };
        }

        private static Dictionary<string, Color> GetOptions(MultiplayerGame game)
        {
            var options = new Dictionary<string, Color>()
            {
                {JoinGame, Color.White},
                {ViewMatchHistory, ColorHelper.HexToColor("#FFE76B")},
                {SpectateGame, ColorHelper.HexToColor("#9B51E0")}
            };

            return options;
        }
    }
}
