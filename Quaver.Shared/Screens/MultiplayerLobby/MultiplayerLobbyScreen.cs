using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Main;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.MultiplayerLobby
{
    public sealed class MultiplayerLobbyScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Lobby;

        /// <summary>
        ///    The currently visible multiplayer games
        /// </summary>
        public Bindable<List<MultiplayerGame>> VisibleGames { get; private set; }

        /// <summary>
        ///     The currently selected multiplayer game
        /// </summary>
        public Bindable<MultiplayerGame> SelectedGame { get; private set; }

        /// <summary>
        /// </summary>
        public MultiplayerLobbyScreen()
        {
            if (MapManager.Selected.Value == null && MapManager.Mapsets.Count != 0)
                MapManager.SelectMapFromMapset(MapManager.Mapsets.First());

            CreateBindableVisibleGames();
            CreateBindableSelectedGame();

            SetRichPresence();
            View = new MultiplayerLobbyScreenView(this);
            ScreenExiting += (sender, args) => OnlineManager.Client?.LeaveLobby();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            OnlineManager.MultiplayerGames?.Clear();
            OnlineManager.Client?.JoinLobby();
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
        public override void Destroy()
        {
            VisibleGames.Dispose();
            SelectedGame.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (Exiting || DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                Exit(() => new MainMenuScreen());
                return;
            }
        }

        /// <summary>
        /// </summary>
        private void CreateBindableVisibleGames() => VisibleGames = new Bindable<List<MultiplayerGame>>(new List<MultiplayerGame>())
        {
            Value = new List<MultiplayerGame>()
        };

        /// <summary>
        /// </summary>
        private void CreateBindableSelectedGame() => SelectedGame = new Bindable<MultiplayerGame>(null);

        /// <summary>
        /// </summary>
        private void SetRichPresence()
        {
            DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
            DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);

            Helpers.RichPresenceHelper.UpdateRichPresence("In the menus", "Multiplayer Lobby");
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InLobby, -1, "", 1, "", 0);
    }
}
