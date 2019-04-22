using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Discord;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Lobby;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multiplayer
{
    public sealed class MultiplayerScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Multiplayer;

        /// <summary>
        ///     The multiplayer game this represents
        /// </summary>
        public MultiplayerGame Game { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="game"></param>
        public MultiplayerScreen(MultiplayerGame game)
        {
            Game = game;

            DiscordHelper.Presence.Details = "Waiting to Start";
            DiscordHelper.Presence.State = $"{game.Name} ({Game.Players.Count} of {Game.MaxPlayers})";
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            View = new MultiplayerScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!Exiting)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                {
                    Exit(() =>
                    {
                        OnlineManager.LeaveGame();
                        return new LobbyScreen();
                    });
                }
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Multiplayer, -1, "", 1, "", 0);
    }
}