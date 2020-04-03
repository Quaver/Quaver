using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.Server.Client;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Downloading;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Selection;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Main
{
    public sealed class MainMenuScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Menu;

        /// <summary>
        ///     Dictates if this is the first ever menu screen load.
        ///     Used to determine if we should auto-connect to the server
        /// </summary>
        public static bool FirstMenuLoad { get; private set; }

        /// <summary>
        /// </summary>
        public MainMenuScreen()
        {
#if  !VISUAL_TESTS
            SetDiscordRichPresence();
#endif
            ModManager.RemoveSpeedMods();

            View = new MainMenuScreenView(this);
        }

        public override void OnFirstUpdate()
        {
            base.OnFirstUpdate();
        }

        public override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        ///     Handles all input for the screen
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Count != 0)
                return;

            HandleKeyPressEscape();
        }

        /// <summary>
        ///     Handles when the user presses escape
        /// </summary>
        private void HandleKeyPressEscape()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                return;

            DialogManager.Show(new QuitDialog());
        }

        /// <summary>
        ///     Exits the screen and goes to single player
        /// </summary>
        public void ExitToSinglePlayer()
        {
            // We have maps in the queue, so we need to go to the import screen first
            if (MapsetImporter.Queue.Count != 0)
            {
                Exit(() => new ImportingScreen());
                return;
            }

            // User has no maps loaded.
            if (MapManager.Mapsets.Count == 0)
            {
                // If they're online, send them to the download screen
                if (OnlineManager.Status.Value == ConnectionStatus.Connected)
                {
                    Exit(() => new DownloadingScreen());
                    return;
                }

                // Not online, just notify them.
                NotificationManager.Show(NotificationLevel.Error, "You have no maps loaded. Try importing some!");
                return;
            }

            Exit(() => new SelectionScreen());
        }

        /// <summary>
        ///     Exits the screen and goes to competitive
        /// </summary>
        public void ExitToCompetitive()
        {
            NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet!");
        }

        /// <summary>
        ///     Exits the screen and goes to multiplayer
        /// </summary>
        public void ExitToMultiplayer()
        {
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Error, "You must be logged in to play multiplayer.");
                return;
            }

            // User has no maps, so send them to the download screen.
            if (MapManager.Mapsets.Count == 0 || MapManager.Selected == null || MapManager.Selected.Value == null)
            {
                if (OnlineManager.Status.Value == ConnectionStatus.Connected)
                {
                    Exit(() => new DownloadingScreen());
                    return;
                }

                NotificationManager.Show(NotificationLevel.Error, "You have no maps loaded. Try importing some!");
                return;
            }

            Exit(() => new MultiplayerLobbyScreen());
        }

        /// <summary>
        ///     Exits the screen to the editor
        /// </summary>
        public void ExitToEditor()
        {
            if (MapManager.Selected == null || MapManager.Selected.Value == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot edit without a map selected.");
                return;
            }

            Exit(() =>
            {
                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track?.Pause();

                try
                {
                    return new EditScreen(MapManager.Selected.Value, AudioEngine.LoadMapAudioTrack(MapManager.Selected.Value));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "Unable to read map file!");
                    return new MainMenuScreen();
                }
            });
        }

        /// <summary>
        /// </summary>
        public void ExitToDownload() => Exit(() => new DownloadingScreen(Type));

        /// <summary>
        /// </summary>
        private void SetDiscordRichPresence()
        {
            DiscordHelper.Presence.Details = "Main Menu";
            DiscordHelper.Presence.State = "In the menus";
            DiscordHelper.Presence.PartySize = 0;
            DiscordHelper.Presence.PartyMax = 0;
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
            DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus()
            => new UserClientStatus(ClientStatus.InMenus, -1, "", (byte) ConfigManager.SelectedGameMode.Value,
                "", (long) ModManager.Mods);
    }
}