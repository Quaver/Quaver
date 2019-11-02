using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Lobby;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.Selection;
using Wobble.Graphics.UI.Buttons;
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
            View = new MainMenuScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            ImportMaps();

            base.Update(gameTime);
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
                    Exit(() => new DownloadScreen());
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
                    Exit(() => new DownloadScreen());
                    return;
                }

                NotificationManager.Show(NotificationLevel.Error, "You have no maps loaded. Try importing some!");
                return;
            }

            Exit(() => new LobbyScreen());
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
                    return new EditorScreen(MapManager.Selected.Value.LoadQua(false));
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
        ///     Automatically starts importing maps if currently in spectator mode
        /// </summary>
        private void ImportMaps()
        {
            // Go to the import screen if we've imported a map not on the select screen
            if (OnlineManager.IsSpectatingSomeone && !Exiting && MapsetImporter.Queue.Count > 0
                || QuaverSettingsDatabaseCache.OutdatedMaps.Count != 0 || MapDatabaseCache.MapsToUpdate.Count != 0)
            {
                Exit(() => new ImportingScreen());
                return;
            }
        }

        /// <summary>
        /// </summary>
        private void SetDiscordRichPresence()
        {
            DiscordHelper.Presence.Details = "Main Menu";
            DiscordHelper.Presence.State = "In the menus";
            DiscordHelper.Presence.PartySize = 0;
            DiscordHelper.Presence.PartyMax = 0;
            DiscordHelper.Presence.EndTimestamp = 0;
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