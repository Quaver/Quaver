using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Lobby
{
    public sealed class LobbyScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Lobby;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public LobbyScreen()
        {
            CheckConnected();
            OnlineManager.JoinLobby();

            DiscordHelper.Presence.Details = "Finding a Game";
            DiscordHelper.Presence.State = "In the Lobby";
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            View = new LobbyScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            foreach (var game in new List<MultiplayerGame>(OnlineManager.MultiplayerGames.Values))
                AddOrUpdateGame(game);

            var view = View as LobbyScreenView;
            view?.MatchContainer.FilterGames("", false);

            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InLobby, -1, "", 1, "", 0);

        /// <summary>
        ///     Checks if the user is connected and returns them to the lobby if they're
        ///     not.
        /// </summary>
        private void CheckConnected()
        {
            if (OnlineManager.Connected)
                return;

            Exit(() =>
            {
                NotificationManager.Show(NotificationLevel.Error, "You must be logged in to join the multiplayer lobby.");
                return new MenuScreen();
            });
        }

        /// <summary>
        ///     Handles all input for this screen
        /// </summary>
        private void HandleInput()
        {
            if (Exiting || DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                ExitToMenu();
        }

        /// <summary>
        ///     Exits the lobby and returns back to the main menu
        /// </summary>
        public void ExitToMenu() => Exit(() =>
        {
            if (OnlineManager.Connected)
                OnlineManager.LeaveLobby();

            return new MenuScreen();
        });

        /// <summary>
        ///     If the match currently isn't in the list, it'll add it.
        ///     Otherwise, it'll update the contents of the game.
        /// </summary>
        /// <param name="game"></param>
        public void AddOrUpdateGame(MultiplayerGame game)
        {
            var view = (LobbyScreenView) View;
            view.MatchContainer.AddOrUpdateGame(game);
        }

        /// <summary>
        ///     Removes the game from the list entirely.
        /// </summary>
        /// <param name="game"></param>
        public void DeleteGame(MultiplayerGame game)
        {
            var view = (LobbyScreenView) View;
            view.MatchContainer.DeleteObject(game);
        }
    }
}