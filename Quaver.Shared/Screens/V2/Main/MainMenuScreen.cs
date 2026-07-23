using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.Server.Client;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Main.Cheats;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Selection.UI.Dialogs;
using Quaver.Shared.Screens.V2.Main.UI;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Main
{
    /// <summary>
    ///     Rewritten main menu screen. Its behavior is intentionally independent from the legacy main menu.
    /// </summary>
    public sealed class MainMenuScreen : QuaverScreen, IPersistentScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Menu;

        private static bool FirstLoadHandled { get; set; }

        private bool OriginalAutoLoadOsuBeatmapsValue { get; }

        private bool FlaggedForOsuImport { get; set; }

        private CheatCodeTheater TheaterCheat { get; }

        private MainMenuJukeboxController Jukebox { get; }

        IReadOnlyCollection<string> IPersistentScreen.PersistentElementKeys { get; } =
            new[] { ScreenNavigation.ElementKey };

        public MainMenuScreen()
        {
            SetRichPresence();
            ModManager.RemoveSpeedMods();

            OriginalAutoLoadOsuBeatmapsValue = ConfigManager.AutoLoadOsuBeatmaps.Value;
            ConfigManager.AutoLoadOsuBeatmaps.ValueChanged += OnAutoLoadOsuBeatmapsChanged;
            TheaterCheat = new CheatCodeTheater();

            if (AudioEngine.MeasuredAudioStartDelay == 0)
                AudioEngine.MeasureAudioStartDelay();

            Jukebox = new MainMenuJukeboxController(this);
            View = new MainMenuScreenView(this);
        }

        public override void OnActivated()
        {
            base.OnActivated();
            ScreenNavigation.EnsureAttached(View.Container);
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
                        NotificationManager.Show(NotificationLevel.Error,
                            LocalizationManager.Get("Screen_Main_AutoLoginFailed"));
                    }
                }

                FirstLoadHandled = true;
            }

            GameBase.Game.GlobalUserInterface.Cursor.Show(1);
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;
            SkinManager.StartWatching();
            ScreenExiting += OnScreenExiting;

            base.OnFirstUpdate();
        }

        public override void Update(GameTime gameTime)
        {
            Jukebox.Update();
            HandleInput(gameTime);
            base.Update(gameTime);
        }

        public override void Destroy()
        {
            ConfigManager.AutoLoadOsuBeatmaps.ValueChanged -= OnAutoLoadOsuBeatmapsChanged;
            ScreenExiting -= OnScreenExiting;
            Jukebox.Destroy();
            TheaterCheat.Destroy();
            base.Destroy();
        }

        public void ExitToSinglePlayer()
        {
            if (MapsetImporter.Queue.Count != 0 || FlaggedForOsuImport)
            {
                Exit(() => new ImportingScreen());
                return;
            }

            if (MapManager.Mapsets.Count == 0)
            {
                if (OnlineManager.Status.Value == ConnectionStatus.Connected)
                {
                    Exit(() => QuaverScreenFactory.CreateDownloading());
                    return;
                }

                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Main_NoMapsLoaded"));
                return;
            }

            Exit(() => QuaverScreenFactory.CreateSelection());
        }

        public void ExitToCompetitive() =>
            NotificationManager.Show(NotificationLevel.Warning,
                LocalizationManager.Get("Screen_Main_NotImplemented"));

        public void ExitToMultiplayer()
        {
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Main_MultiplayerLoginRequired"));
                return;
            }

            if (MapManager.Mapsets.Count == 0)
            {
                if (OnlineManager.Status.Value == ConnectionStatus.Connected)
                {
                    Exit(() => QuaverScreenFactory.CreateDownloading());
                    return;
                }

                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Main_NoMapsLoaded"));
                return;
            }

            Exit(() => QuaverScreenFactory.CreateMultiplayerLobby());
        }

        public void ExitToEditor()
        {
            if (MapManager.Selected?.Value == null)
            {
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Main_EditorMapRequired"));
                return;
            }

            Exit(() =>
            {
                if (AudioEngine.Track?.IsPlaying == true)
                    AudioEngine.Track.Pause();

                try
                {
                    return new EditScreen(MapManager.Selected.Value,
                        AudioEngine.LoadMapAudioTrack(MapManager.Selected.Value));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error,
                        LocalizationManager.Get("Screen_Main_MapReadFailed"));
                    return QuaverScreenFactory.CreateMainMenu();
                }
            });
        }

        public void ExitToDownload() => Exit(() => QuaverScreenFactory.CreateDownloading(Type));

        public override UserClientStatus GetClientStatus() =>
            new UserClientStatus(ClientStatus.InMenus, -1, "", (byte) ConfigManager.SelectedGameMode.Value,
                "", (long) ModManager.Mods);

        private void HandleInput(GameTime gameTime)
        {
            if (Exiting || DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Show(new QuitDialog());

            if (!KeyboardManager.IsCtrlDown() && KeyboardManager.IsUniqueKeyPress(Keys.F5))
                DialogManager.Show(new RefreshDialog());

            TheaterCheat.Update(gameTime);
        }

        private static void SetRichPresence()
        {
            DiscordHelper.Presence.PartySize = 0;
            DiscordHelper.Presence.PartyMax = 0;
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordHelper.Presence.LargeImageText =
                OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.Presence.SmallImageKey =
                ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
            DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);

            SteamManager.SetRichPresence("steam_player_group", null);
            SteamManager.SetRichPresence("steam_player_group_size", null);
            Helpers.RichPresenceHelper.UpdateRichPresence("In the menus", "Main Menu");
        }

        private static void OnScreenExiting(object sender, EventArgs e) => SkinManager.StopWatching();

        private void OnAutoLoadOsuBeatmapsChanged(object sender, BindableValueChangedEventArgs<bool> e) =>
            FlaggedForOsuImport = e.Value != OriginalAutoLoadOsuBeatmapsValue;
    }
}
