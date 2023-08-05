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
using Quaver.Shared.Screens.Main.Cheats;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI.Dialogs;
using Wobble;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Bindables;

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
        public static bool FirstLoadHandled { get; private set; }

        /// <summary>
        ///	</summary>
        private bool OriginalAutoLoadOsuBeatmapsValue { get; }

        /// <summary>
        ///     If true, the user will be taken to the import screen where all of their maps from other games
        ///     will be loaded. This is to allow users with 0 maps installed to load all of them without restarting
        ///	</summary>
        private bool FlaggedForOsuImport { get; set; }

        private CheatCodeTheater TheaterCheat { get; set; }

        /// <summary>
        /// </summary>
        public MainMenuScreen()
        {
#if  !VISUAL_TESTS
            SetRichPresence();
#endif
            ModManager.RemoveSpeedMods();

            OriginalAutoLoadOsuBeatmapsValue = ConfigManager.AutoLoadOsuBeatmaps.Value;
            ConfigManager.AutoLoadOsuBeatmaps.ValueChanged += OnAutoLoadOsuBeatmapsChanged;
            TheaterCheat = new CheatCodeTheater();
            
            View = new MainMenuScreenView(this);
        }

        public override void OnFirstUpdate()
        {
            if (!FirstLoadHandled)
            {
                if (ConfigManager.AutoLoginToServer.Value)
                {
                    try
                    {
                        OnlineManager.Login();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, "Failed to automatically login!");
                    }
                }

                FirstLoadHandled = true;
            }

            GameBase.Game.GlobalUserInterface.Cursor.Show(1);
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;

            base.OnFirstUpdate();
        }

        public override void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
            base.Update(gameTime);
        }

        public override void Destroy()
        {
            ConfigManager.AutoLoadOsuBeatmaps.ValueChanged -= OnAutoLoadOsuBeatmapsChanged;
            TheaterCheat.Destroy();
            base.Destroy();
        }

        /// <summary>
        ///     Handles all input for the screen
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            if (Exiting)
                return;

            if (DialogManager.Dialogs.Count != 0)
                return;

            HandleKeyPressEscape();
            HandleKeyPressF5();
            TheaterCheat.Update(gameTime);
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
        ///		Sets the flag to begin a force refresh of mapsets when entering singleplayer.
        ///	</summary>
        private void HandleKeyPressF5()
        {
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                return;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.F5))
                return;

            DialogManager.Show(new RefreshDialog());
        }

        /// <summary>
        ///     Exits the screen and goes to single player
        /// </summary>
        public void ExitToSinglePlayer()
        {
            // We have maps in the queue, so we need to go to the import screen first
            if (MapsetImporter.Queue.Count != 0 || FlaggedForOsuImport)
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
        private void SetRichPresence()
        {
            DiscordHelper.Presence.PartySize = 0;
            DiscordHelper.Presence.PartyMax = 0;
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
            DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);

            SteamManager.SetRichPresence("steam_player_group", null);
            SteamManager.SetRichPresence("steam_player_group_size", null);

            Helpers.RichPresenceHelper.UpdateRichPresence("In the menus", "Main Menu");
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus()
            => new UserClientStatus(ClientStatus.InMenus, -1, "", (byte) ConfigManager.SelectedGameMode.Value,
                "", (long) ModManager.Mods);
        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnAutoLoadOsuBeatmapsChanged(object sender, BindableValueChangedEventArgs<bool> e)
            => FlaggedForOsuImport = e.Value != OriginalAutoLoadOsuBeatmapsValue;
    }
}