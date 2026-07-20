/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.Server.Client.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database;
using Quaver.Shared.Database.BlockedUsers;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Chatting;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Graphics.Overlays.Volume;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Helpers;
using Quaver.Shared.Localization;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Imgur;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Downloading;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Initialization;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Options;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Tests.AutoMods;
using Quaver.Shared.Screens.Tests.ButtonPerformance;
using Quaver.Shared.Screens.Tests.Border;
using Quaver.Shared.Screens.Tests.Chat;
using Quaver.Shared.Screens.Tests.CheckboxContainers;
using Quaver.Shared.Screens.Tests.CreatePlaylists;
using Quaver.Shared.Screens.Tests.DifficultyBars;
using Quaver.Shared.Screens.Tests.DifficultyGraph;
using Quaver.Shared.Screens.Tests.DrawableLeaderboardScores;
using Quaver.Shared.Screens.Tests.DrawableMaps;
using Quaver.Shared.Screens.Tests.DrawableMapsets;
using Quaver.Shared.Screens.Tests.DrawableMapsetsMultiple;
using Quaver.Shared.Screens.Tests.DrawablePlaylists;
using Quaver.Shared.Screens.Tests.Dropdowns;
using Quaver.Shared.Screens.Tests.Editor;
using Quaver.Shared.Screens.Tests.FilterPanel;
using Quaver.Shared.Screens.Tests.Jukebox;
using Quaver.Shared.Screens.Tests.Leaderboards;
using Quaver.Shared.Screens.Tests.LeaderboardWithMaps;
using Quaver.Shared.Screens.Tests.MapsetScrollContainers;
using Quaver.Shared.Screens.Tests.MapScrollContainers;
using Quaver.Shared.Screens.Tests.ModifierSelectors;
using Quaver.Shared.Screens.Tests.YesNoDialog;
using Quaver.Shared.Screens.Tests.Footer;
using Quaver.Shared.Screens.Tests.ListenerLists;
using Quaver.Shared.Screens.Tests.Luas;
using Quaver.Shared.Screens.Tests.MenuJukebox;
using Quaver.Shared.Screens.Tests.Notifications;
using Quaver.Shared.Screens.Tests.OnlineHubDownloads;
using Quaver.Shared.Screens.Tests.OnlineHubs;
using Quaver.Shared.Screens.Tests.Options;
using Quaver.Shared.Screens.Tests.Profiles;
using Quaver.Shared.Screens.Tests.ReplayControllers;
using Quaver.Shared.Screens.Tests.Results;
using Quaver.Shared.Screens.Tests.ResultsMulti;
using Quaver.Shared.Screens.Tests.Tournaments;
using Quaver.Shared.Screens.Tests.Volume;
using Quaver.Shared.Screens.Theater;
using Quaver.Shared.Skinning;
using Quaver.Shared.Window;
using Steamworks;
using Wobble;
using Wobble.Audio;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Extended.HotReload;
using Wobble.Extended.HotReload.Screens;
using Wobble.Graphics;
using Wobble.Graphics.UI.Debugging;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Input;
using Wobble.IO;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Platform;
using Wobble.Window;
using Version = YamlDotNet.Core.Version;

namespace Quaver.Shared
{
#if VISUAL_TESTS
    public class QuaverGame : HotLoaderGame
#else
    public class QuaverGame : WobbleGame
#endif
    {
        #region Win32 multi-monitor P/Invoke

        /// <summary>
        ///     Win32 flag that returns the monitor nearest to the given point.
        /// </summary>
        private const int MONITOR_DEFAULTTONEAREST = 2;

        /// <summary>
        ///     Win32 POINT structure used for multi-monitor lookups.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        /// <summary>
        ///     Win32 RECT structure representing a rectangle with left, top, right, and bottom edges.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        /// <summary>
        ///     Win32 MONITORINFO structure containing monitor geometry and work area bounds.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public int Flags;
        }

        /// <summary>
        ///     Retrieves a handle to the display monitor that contains or is nearest to the specified point.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, int flags);

        /// <summary>
        ///     Retrieves information about a display monitor.
        /// </summary>
        /// <param name="hMonitor"></param>
        /// <param name="lpmi"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        #endregion

        #region SDL2 multi-monitor P/Invoke

        /// <summary>
        ///     SDL2 rectangle structure used for display bounds queries.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SDL_Rect { public int x, y, w, h; }

        /// <summary>
        ///     Returns the number of available video displays.
        /// </summary>
        /// <returns></returns>
        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_GetNumVideoDisplays();

        /// <summary>
        ///     Gets the desktop area bounds of a display, with position relative to the primary monitor.
        /// </summary>
        /// <param name="displayIndex"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_GetDisplayBounds(int displayIndex, out SDL_Rect rect);

        #endregion

        /// <summary>
        ///     Scaling factor for skin values and scroll speed to convert them to the UI redesign coordinate system.
        /// </summary>
        public const float SkinScalingFactor = 1920f / 1366;

        /// <summary>
        ///     The bounds of the monitor the window was on right before entering full screen.
        ///     <see cref="GameWindow.Position"/> always reports (0, 0) while actually in full screen mode,
        ///     so this is captured beforehand (while still windowed) and used to restore the window to the
        ///     correct monitor when full screen is turned back off.
        /// </summary>
        private MonitorBounds? _fullScreenMonitorBounds;

        /// <summary>
        ///     Debounce delay (ms) for windowed resize events, so an animated snap/tile doesn't
        ///     reload the screen on every intermediate frame.
        /// </summary>
        private const double ClientSizeChangeDebounceMs = 200;

        /// <summary>
        ///     Whether a resize is waiting for <see cref="ClientSizeChangeDebounceMs"/> to elapse.
        /// </summary>
        private bool _pendingClientSizeChange;

        /// <summary>
        ///     <see cref="TimeRunning"/> at the last <see cref="Window.ClientSizeChanged"/> event.
        /// </summary>
        private long _lastClientSizeChangeTime;

        /// <summary>
        ///     Backbuffer size before the pending resize sequence started. Wobble's own resize handler
        ///     syncs the backbuffer immediately, so by the time the debounce fires, comparing against
        ///     the live size would miss the change.
        /// </summary>
        private Point _pendingOldBackBufferSize;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override bool IsReadyToUpdate { get; set; }

        /// <summary>
        ///     The volume controller for the game.
        /// </summary>
        public VolumeControl VolumeController { get; private set; }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public OnlineHub OnlineHub { get; private set; }

        /// <summary>
        /// </summary>
        public OnlineChat OnlineChat { get; private set; }

        /// <summary>
        /// </summary>
        public FpsCounter Fps { get; private set; }

        /// <summary>
        ///     The current activated screen.
        /// </summary>
        public QuaverScreen CurrentScreen { get; set; }

        /// <summary>
        ///     Unique identifier of the client's assembly version.
        /// </summary>
        protected AssemblyName AssemblyName => Assembly.GetEntryAssembly()?.GetName() ?? new AssemblyName { Version = new System.Version() };

        /// <summary>
        ///     Determines if the build is deployed/an official release.
        ///     By default, it's 0.0.0.0 - Anything else is considered deployed.
        /// </summary>
        public bool IsDeployedBuild => AssemblyName.Version.Major != 0 || AssemblyName.Version.Minor != 0 || AssemblyName.Version.Revision != 0 ||
                                        AssemblyName.Version.Build != 0;

        /// <summary>
        ///     Stringified version name of the client.
        /// </summary>
        public string Version
        {
            get
            {
                if (!IsDeployedBuild)
                    return "Local Development Build";

                var assembly = AssemblyName;
                return $@"{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build}.{assembly.Version.Revision}";
            }
        }

        /// <summary>
        ///     Used to detect when to limit FPS if the user's window isn't active.
        /// </summary>
        private bool WindowActiveInPreviousFrame { get; set; }

        /// <summary>
        ///     Used to detect when to mute/restore audio if the user's window isn't active.
        /// </summary>
        private bool WindowActiveInPreviousFrameForAudio { get; set; } = true;

        /// <summary>
        ///     Used to detect when to switch between gameplay/menu music volume.
        /// </summary>
        private bool WasUsingGameplayMusicVolumeInPreviousFrame { get; set; }

        /// <summary>
        ///     The music volume that <see cref="HandleMusicVolumeFade"/> is currently fading
        ///     <see cref="AudioTrack.GlobalVolume"/> towards.
        /// </summary>
        private float TargetMusicVolume { get; set; }

        /// <summary>
        ///     The music volume the current fade started from.
        /// </summary>
        private float MusicVolumeFadeStart { get; set; }

        /// <summary>
        ///     How long in milliseconds the current fade has been running for.
        /// </summary>
        private double MusicVolumeFadeElapsed { get; set; }

        /// <summary>
        ///     How long in milliseconds it takes to fade between music volumes
        ///     (e.g. when switching between gameplay/menu music volume or when muting).
        /// </summary>
        private const int MusicVolumeFadeTime = 400;

        /// <summary>
        ///     FPS to use when reducing rendering for an inactive visible window.
        /// </summary>
        private const int InactiveWindowFpsLimit = 30;

        /// <summary>
        ///     Keeps SDL's Cocoa event pump from being throttled by fixed timestep sleeps on macOS.
        /// </summary>
        private bool MacOsCocoaEventLoopDrawLimiter { get; set; }

        /// <summary>
        /// </summary>
        private double MacOsCocoaEventLoopDrawLimiterTicks { get; set; }

        /// <summary>
        /// </summary>
        private long MacOsCocoaEventLoopLastDrawTicks { get; set; }

        /// <summary>
        ///     Sometimes we'd like to perform actions on the first update, such as
        ///     creating <see cref="OnlineHub"/>
        /// </summary>
        public bool FirstUpdateCalled { get; private set; }

#if VISUAL_TESTS
        /// <summary>
        ///     The visual screen type in the previous frame
        /// </summary>
        private Type LastVisualTestScreenType { get; set; }

        /// <summary>
        /// </summary>
        private Dictionary<string, Type> VisualTests { get; } = new Dictionary<string, Type>()
        {
            {"AutoMod", typeof(AutoModTestScreen)},
            {"Main Menu", typeof(MainMenuScreen)},
            {"ResultsScreen (Multi)", typeof(TestResultsMultiScreen)},
            {"ResultsScreen", typeof(TestResultsScreen)},
            {"TournamentOverlay", typeof(TestTournamentOverlayScreen)},
            {"Editor", typeof(TestEditorScreen)},
            {"LuaImGui", typeof(TestLuaScriptingScreen)},
            {"LocalProfileContainer", typeof(TestUserProfileContainerScreen)},
            {"DifficultyGraph", typeof(TestDifficultyGraphScreen)},
            {"DownloadingScreen", typeof(DownloadingScreen)},
            {"Dropdown", typeof(DropdownTestScreen)},
            {"MenuBorder", typeof(MenuBorderTestScreen)},
            {"OptionsMenu", typeof(OptionsTestScreen)},
            {"VolumeController", typeof(TestVolumeControlScreen)},
            {"ReplayController", typeof(TestReplayControllerScreen)},
            {"SelectFilterPanel", typeof(FilterPanelTestScreen)},
            {"SelectJukebox", typeof(TestSelectJukeboxScreen)},
            {"DrawableMapset", typeof(TestMapsetScreen)},
            {"DrawableMapset (Multiple)", typeof(TestMapsetsMultipleScreen)},
            {"DifficultyBarDisplay", typeof(TestScreenDifficultyBar)},
            {"MapsetScrollContainer", typeof(TestScreenMapsetScrollContainer)},
            {"DrawableMap", typeof(TestDrawableMapScreen)},
            {"MapScrollContainer", typeof(TestScreenMapScrollContainer)},
            {"Leaderboard", typeof(TestLeaderboardScreen)},
            {"Leaderboard + Maps", typeof(TestLeaderboardWithMapsScreen)},
            {"DrawableLeaderboardScore", typeof(TestScreenDrawableLeaderboardScore)},
            {"ModifierSelector", typeof(TestModifierSelectorScreen)},
            {"CreatePlaylistDialog", typeof(TestScreenCreatePlaylist)},
            {"SelectionScreen", typeof(SelectionScreen)},
            {"YesNoDialog", typeof(TestYesNoDialogScreen)},
            {"DrawablePlaylist", typeof(TestScreenDrawablePlaylist)},
            {"MenuFooterJukebox", typeof(TestScreenMenuJukebox)},
            {"MusicPlayerScreen", typeof(MusicPlayerScreen)},
            {"DrawableListenerList", typeof(TestScreenListenerList)},
            {"OnlineHub", typeof(TestScreenOnlineHub)},
            {"OnlineHubDownloads", typeof(TestOnlineHubDownloadsScreen)},
            {"Notifications", typeof(TestNotificationScreen)},
            {"ChatOverlay", typeof(TestChatScreen)},
            {"MultiplayerGameScreen", typeof(MultiplayerGameScreen)},
            {"MultiplayerLobbyScreen", typeof(MultiplayerLobbyScreen)},
            {"CheckboxContainer", typeof(TestCheckboxContainerScreen)},
            {"ButtonPerformance", typeof(ButtonPerformanceTestScreen)},
        };

        public QuaverGame(HotLoader hl) : base(hl, ConfigureSdlVideoBackend())
#else
        public QuaverGame() : base(ConfigureSdlVideoBackend())
#endif
        {
            Content.RootDirectory = "Content";

            if (Environment.GetEnvironmentVariable("QUAVER_LOGLEVEL") is null)
                Logger.MinimumLogLevel = IsDeployedBuild ? LogLevel.Important : LogLevel.Debug;
        }

        /// <summary>
        ///     Applies SDL video backend preferences before MonoGame initializes SDL.
        /// </summary>
        /// <returns></returns>
        private static bool ConfigureSdlVideoBackend()
        {
            if (PreferCocoaEventLoop() &&
                string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SDL_VIDEODRIVER")))
            {
                Environment.SetEnvironmentVariable("SDL_VIDEODRIVER", "cocoa");
            }

            return ConfigManager.PreferWayland.Value;
        }

        /// <summary>
        ///     Whether to use the macOS Cocoa SDL event loop behavior.
        /// </summary>
        /// <returns></returns>
        private static bool PreferCocoaEventLoop() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && ConfigManager.PreferCocoaEventLoop.Value;

        /// <inheritdoc />
        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            QuaverScreenManager.Initialize();
            WindowManager.ChangeBaseResolution(new Vector2(1920, 1080));
            QuaverLocalization.Configure(ConfigManager.Language.Value);
            Resources.AddStore(new DllResourceStore("Quaver.Resources.dll"));

#if VISUAL_TESTS
            Fonts.LoadWobbleFonts();
#endif

            Graphics.IsFullScreen = ConfigManager.WindowFullScreen.Value;
            Window.IsBorderless = ConfigManager.WindowBorderless.Value;
            ChangeResolution();

            // Don't change the actual display mode. Especially considering our support for arbitrary resolutions, this
            // can lead to completely locking up user's session (on Linux).
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Graphics.HardwareModeSwitch = false;

            // Apply all graphics changes
            Graphics.ApplyChanges();

            // Handle file dropped event.
            Window.FileDropped += MapsetImporter.OnFileDropped;
            Window.ClientSizeChanged += OnClientSizeChanged;
            AudioManager.OutputDeviceChanged += OnAudioOutputDeviceChanged;
            AudioManager.ShouldSkipLostOutputDeviceCheck = () => CurrentScreen?.Type == QuaverScreenType.Gameplay;

            DevicePeriod = ConfigManager.DevicePeriod.Value;
            DeviceBufferLength = DevicePeriod * ConfigManager.DeviceBufferLengthMultiplier.Value;

            base.Initialize();
        }

        /// <inheritdoc />
        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            Logger.Important($"Currently running Quaver version: `{Version}`", LogType.Runtime);
            IsReadyToUpdate = true;

#if VISUAL_TESTS
            Window.Title = $"Quaver Visual Test Runner";
            new InitializationScreen().OnFirstUpdate();
#else
            Window.Title = !IsDeployedBuild ? $"Quaver - {Version}" : $"Quaver v{Version}";
            QuaverScreenManager.ScheduleScreenChange(() => new InitializationScreen(), true);
#endif
        }

        /// <inheritdoc />
        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            AudioManager.OutputDeviceChanged -= OnAudioOutputDeviceChanged;
            AudioManager.ShouldSkipLostOutputDeviceCheck = null;
            ConfigManager.WriteConfigFileAsync().Wait();
            Transitioner.Dispose();
            DiscordHelper.Shutdown();
            TooltipManager.TargetEligibilityFilter = null;
            base.UnloadContent();

            if (SteamManager.IsInitialized)
                SteamAPI.Shutdown();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            if (!IsReadyToUpdate)
                return;

            base.Update(gameTime);

            if (SteamManager.IsInitialized)
                SteamAPI.RunCallbacks();

            if (!FirstUpdateCalled)
            {
                InitializeFpsLimiting();

                // Create the online hub on the first update, since it uses text, and we have to wait for things to
                // be initialized
                OnlineHub = new OnlineHub();
                OnlineChat = new OnlineChat();
                TooltipManager.TargetEligibilityFilter = target => OnlineChat.AllowsTooltip(target);
                VolumeController = new VolumeControl();
                FirstUpdateCalled = true;
            }

            // Run scheduled background tasks
            CommonTaskScheduler.Run();

            BackgroundHelper.Update(gameTime);
            DialogManager.Update(gameTime);

            HandleGlobalInput(gameTime);
            HandleOnlineHubInput();

            NotificationManager.Update(gameTime);
            VolumeController?.Update(gameTime);
            Transitioner.Update(gameTime);

#if VISUAL_TESTS
            SetVisualTestingPresence();
#endif

            SkinManager.HandleSkinReloading();
            LimitFpsOnInactiveWindow();
            HandleMuteAudioOnWindowInactive();
            HandleMusicVolumeFade(gameTime);
            UpdateFpsCounterPosition();
            HandlePendingClientSizeChange();

            Window.AllowUserResizing = QuaverWindowManager.CanChangeResolutionOnScene;
        }

        /// <summary>
        ///     Applies a pending resize once <see cref="ClientSizeChangeDebounceMs"/> has elapsed.
        /// </summary>
        private void HandlePendingClientSizeChange()
        {
            if (!_pendingClientSizeChange)
                return;

            if (TimeRunning - _lastClientSizeChangeTime < ClientSizeChangeDebounceMs)
                return;

            _pendingClientSizeChange = false;

            ChangeResolution(false, _pendingOldBackBufferSize);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether the game should draw this update.
        /// </summary>
        protected override bool BeginDraw() => base.BeginDraw() && ShouldRunMacOsDraw();

        /// <inheritdoc />
        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            if (!IsReadyToUpdate)
                return;

            base.Draw(gameTime);

            // Draw dialogs
            DialogManager.Draw(gameTime);

            NotificationManager.Draw(gameTime);
            VolumeController?.Draw(gameTime);
            GlobalUserInterface.Draw(gameTime);

            // F8 chat belongs to global UI, which draws after Wobble's normal tooltip layer.
            if (OnlineChat?.IsOpen == true)
                TooltipManager.Draw(gameTime);

            Transitioner.Draw(gameTime);

            ClearAlphaChannel(gameTime);
        }

        /// <summary>
        ///     Performs any initial setup the game needs to run.
        /// </summary>
        public void PerformGameSetup()
        {
            DeleteTemporaryFiles();

            SetAudioDevice(true);
            DatabaseManager.Initialize();
            ScoreDatabaseCache.CreateTable();
            MapDatabaseCache.Load(false);
            QuaverSettingsDatabaseCache.Initialize();
            JudgementWindowsDatabaseCache.Load();
            UserProfileDatabaseCache.Load();
            BlockedUsers.Load();

            // Force garabge collection.
            GC.Collect();

            // Start watching for mapset changes in the folder.
            MapsetImporter.WatchForChanges();

            // Initially set the global volume.
            UpdateGlobalVolume(true);

            ConfigManager.VolumeGlobal.ValueChanged += (sender, e) => UpdateGlobalVolume();
            ConfigManager.VolumeMusic.ValueChanged += (sender, e) => UpdateGlobalVolume();
            ConfigManager.VolumeMenuMusic.ValueChanged += (sender, e) => UpdateGlobalVolume();
            ConfigManager.VolumeEffect.ValueChanged += (sender, e) => UpdateGlobalVolume();
            ConfigManager.MuteAudioOnWindowInactive.ValueChanged += (sender, e) => UpdateGlobalVolume();

            ConfigManager.Pitched.ValueChanged += (sender, e) =>
            {
                if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed)
                    AudioEngine.Track.ApplyRate(e.Value);
            };

            ConfigManager.FpsLimiterType.ValueChanged += (sender, e) => InitializeFpsLimiting();
            ConfigManager.CustomFpsLimit.ValueChanged += (sender, e) => InitializeFpsLimiting();
            ConfigManager.PreferCocoaEventLoop.ValueChanged += (sender, e) => InitializeFpsLimiting();
            ConfigManager.Language.ValueChanged += (sender, e) =>
            {
                QuaverLocalization.SetCurrentCulture(e.Value);
                Fonts.ReloadCjkFontFace(e.Value);
                NotificationManager.Show(NotificationLevel.Info,
                    LocalizationManager.Get("Notification_LanguageChangeRequiresScreenChange"));
            };
            
            ConfigManager.WindowFullScreen.ValueChanged += (sender, e) =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    NotificationManager.Show(NotificationLevel.Info, "Full screen is not supported on macOS. Use the borderless window mode instead.");
                    
                    ConfigManager.WindowFullScreen.ChangeWithoutTrigger(false);
                }
                else if (e.Value)
                {
                    var centerX = Window.Position.X + Window.ClientBounds.Width / 2;
                    var centerY = Window.Position.Y + Window.ClientBounds.Height / 2;
                    _fullScreenMonitorBounds = GetMonitorBoundsAtPoint(centerX, centerY);

                    Graphics.IsFullScreen = true;
                }
                else
                {
                    Graphics.IsFullScreen = false;
                    Graphics.ApplyChanges();

                    if (_fullScreenMonitorBounds.HasValue)
                        CenterWindowOnMonitor(_fullScreenMonitorBounds.Value, ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);

                    _fullScreenMonitorBounds = null;
                    Window.IsBorderless = ConfigManager.WindowBorderless.Value;
                }
            };
            
            ConfigManager.WindowBorderless.ValueChanged += (sender, e) => Window.IsBorderless = e.Value;
            ConfigManager.SelectedGameMode.ValueChanged += (sender, args) =>
            {
                DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(args.Value);
                DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
                DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);
                DiscordHelper.UpdatePresence();
            };

            ConfigManager.EnableHighProcessPriority.ValueChanged += (sender, args) => SetProcessPriority();

            // Handle discord rich presence.
            DiscordHelper.Initialize("376180410490552320");
            DiscordHelper.Presence = new DiscordRpc.RichPresence()
            {
                LargeImageKey = "quaver",
                LargeImageText = ConfigManager.Username.Value,
                EndTimestamp = 0
            };

#if VISUAL_TESTS
            DiscordHelper.Presence.StartTimestamp = (long) (TimeHelper.GetUnixTimestampMilliseconds() / 1000);
            DiscordHelper.Presence.State = "Visual Testing";
#endif
            DiscordHelper.UpdatePresence();

            MapManager.Selected.ValueChanged += (sender, args) =>
            {
                if (MapManager.RecentlyPlayed.Contains(args.Value))
                    MapManager.RecentlyPlayed.Remove(args.Value);

                MapManager.RecentlyPlayed.Add(args.Value);
            };

            InactiveSleepTime = GetInactiveSleepTime();
        }

        /// <summary>
        ///     Deletes all of the temporary files for the game if they exist.
        /// </summary>
        private static void DeleteTemporaryFiles()
        {
            try
            {
                foreach (var file in new DirectoryInfo(ConfigManager.TempDirectory).GetFiles("*", SearchOption.AllDirectories))
                    file.Delete();

                foreach (var dir in new DirectoryInfo(ConfigManager.TempDirectory).GetDirectories("*", SearchOption.AllDirectories))
                    dir.Delete(true);
            }
            catch (Exception)
            {
                // ignored
            }

            // Create a directory that displays the "Now playing" song.
            Directory.CreateDirectory($"{ConfigManager.TempDirectory}/Now Playing");
        }

        /// <summary>
        ///     Creates the FPS counter to display on a global state.
        /// </summary>
        public void CreateFpsCounter()
        {
            Fps = new FpsCounter(FontManager.GetWobbleFont(Fonts.InterBold), 18)
            {
                Parent = GlobalUserInterface,
                Alignment = Alignment.BotRight,
                Size = new ScalableVector2(70, 30),
                X = -14,
                Visible = false
            };

            ShowFpsCounter(Fps);
            ConfigManager.FpsCounter.ValueChanged += (o, e) => ShowFpsCounter(Fps);
        }

        /// <summary>
        ///     Shows the FPS counter based on the current config variable.
        /// </summary>
        private static void ShowFpsCounter(FpsCounter counter) => counter.Visible = ConfigManager.FpsCounter.Value;

        /// <summary>
        ///     Uses a custom fps config
        /// </summary>
        /// <param name="fpsLimitType"></param>
        /// <param name="customFpsLimit"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetFps(FpsLimitType fpsLimitType, int customFpsLimit)
        {
            MacOsCocoaEventLoopDrawLimiter = false;

            switch (fpsLimitType)
            {
                case FpsLimitType.Unlimited:
                    Graphics.SynchronizeWithVerticalRetrace = false;
                    IsFixedTimeStep = false;
                    WaylandVsync = false;
                    break;
                case FpsLimitType.Limited:
                    Graphics.SynchronizeWithVerticalRetrace = false;
                    if (PreferCocoaEventLoop())
                        SetMacOsCocoaEventLoopDrawLimiter(240);
                    else
                    {
                        IsFixedTimeStep = true;
                        TargetElapsedTime = TimeSpan.FromSeconds(1d / 240d);
                    }
                    WaylandVsync = false;
                    break;
                case FpsLimitType.Vsync:
                    Graphics.SynchronizeWithVerticalRetrace = true;
                    IsFixedTimeStep = false;
                    WaylandVsync = false;
                    break;
                case FpsLimitType.WaylandVsync:
                    Graphics.SynchronizeWithVerticalRetrace = false;
                    IsFixedTimeStep = false;
                    WaylandVsync = true;
                    break;
                case FpsLimitType.Custom:
                    if (customFpsLimit <= 0)
                        throw new ArgumentOutOfRangeException(nameof(customFpsLimit), customFpsLimit,
                            "Custom FPS limit must be greater than zero.");

                    Graphics.SynchronizeWithVerticalRetrace = false;
                    if (PreferCocoaEventLoop())
                        SetMacOsCocoaEventLoopDrawLimiter(customFpsLimit);
                    else
                    {
                        TargetElapsedTime = TimeSpan.FromSeconds(1d / customFpsLimit);
                        IsFixedTimeStep = true;
                    }
                    WaylandVsync = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Graphics.ApplyChanges();
        }

        /// <summary>
        ///     Limits rendered frames on macOS without using MonoGame's fixed timestep sleep.
        /// </summary>
        /// <param name="fps"></param>
        private void SetMacOsCocoaEventLoopDrawLimiter(int fps)
        {
            IsFixedTimeStep = false;
            MacOsCocoaEventLoopDrawLimiter = true;
            MacOsCocoaEventLoopDrawLimiterTicks = Stopwatch.Frequency / (double)fps;
            MacOsCocoaEventLoopLastDrawTicks = 0;
        }

        /// <summary>
        ///     Allows draws only when the selected macOS FPS interval elapses, while updates still pump SDL events.
        /// </summary>
        /// <returns></returns>
        private bool ShouldRunMacOsDraw()
        {
            var limitInactiveDraws = ShouldLimitMacOsInactiveDraws();

            if (!MacOsCocoaEventLoopDrawLimiter && !limitInactiveDraws)
                return true;

            var now = Stopwatch.GetTimestamp();
            var limiterTicks = limitInactiveDraws
                ? Stopwatch.Frequency / (double)InactiveWindowFpsLimit
                : MacOsCocoaEventLoopDrawLimiterTicks;

            if (MacOsCocoaEventLoopLastDrawTicks != 0 &&
                now - MacOsCocoaEventLoopLastDrawTicks < limiterTicks)
                return false;

            MacOsCocoaEventLoopLastDrawTicks = now;
            return true;
        }

        /// <summary>
        ///     Whether to throttle macOS inactive-window rendering without sleeping the Cocoa event loop.
        /// </summary>
        /// <returns></returns>
        private bool ShouldLimitMacOsInactiveDraws() => PreferCocoaEventLoop() &&
                                                        ConfigManager.LowerFpsOnWindowInactive.Value &&
                                                        !IsActive &&
                                                        OtherGameMapDatabaseCache.OnSyncableScreen() &&
                                                        !(CurrentScreen != null && CurrentScreen.Exiting);

        /// <summary>
        ///     Gets the sleep time used when the inactive-window limiter is allowed to throttle the full game loop.
        /// </summary>
        /// <returns></returns>
        private static TimeSpan GetInactiveSleepTime()
        {
            if (!ConfigManager.LowerFpsOnWindowInactive.Value || PreferCocoaEventLoop())
                return TimeSpan.Zero;

            return TimeSpan.FromSeconds(1d / InactiveWindowFpsLimit);
        }

        /// <summary>
        ///    Handles limiting/unlimiting FPS based on user config
        /// </summary>
        public void InitializeFpsLimiting()
        {
            SetFps(ConfigManager.FpsLimiterType.Value, ConfigManager.CustomFpsLimit.Value);
        }

        /// <summary>
        ///     Handles input's that can be executed everywhere.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleGlobalInput(GameTime gameTime)
        {
            HandleKeyPressF7();
            HandleKeyPressCtrlO();
            HandleKeyPressCtrlS();
            HandleKeyPressAltEnter();
            HandleKeyPressScreenshot();
            HandleKeyPressCtrlP();
        }

        private void HandleKeyPressCtrlP()
        {
            if (!KeyboardManager.IsCtrlDown())
                return;

            switch (CurrentScreen?.Type)
            {
                case QuaverScreenType.Gameplay:
                case QuaverScreenType.Theatre:
                    break;
                default:
                    // Pause/Unpause music
                    if (KeyboardManager.IsUniqueKeyPress(Keys.P) && AudioEngine.Track != null && !AudioEngine.Track.IsDisposed)
                    {
                        if (AudioEngine.Track.IsPaused)
                        {
                            AudioEngine.Track.Play();
                            NotificationManager.Show(NotificationLevel.Info, "Music Unpaused");
                        }
                        else if (AudioEngine.Track.IsPlaying)
                        {
                            AudioEngine.Track.Pause();
                            NotificationManager.Show(NotificationLevel.Info, "Music Paused");
                        }
                    }
                    break;
            }
        }

        /// <summary>
        ///     Handles when the user presses the F7 button
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void HandleKeyPressF7()
        {
            // Handles FPS limiter changes
            if (!KeyboardManager.IsUniqueKeyPress(Keys.F7))
                return;

            var availableFpsLimitTypes = GetAvailableFpsLimitTypes();
            var index = availableFpsLimitTypes.IndexOf(ConfigManager.FpsLimiterType.Value);

            if (index + 1 < availableFpsLimitTypes.Count)
                ConfigManager.FpsLimiterType.Value = availableFpsLimitTypes[index + 1];
            else
                ConfigManager.FpsLimiterType.Value = FpsLimitType.Unlimited;

            switch (ConfigManager.FpsLimiterType.Value)
            {
                case FpsLimitType.Unlimited:
                    NotificationManager.Show(NotificationLevel.Info, "FPS is now unlimited.");
                    break;
                case FpsLimitType.Limited:
                    NotificationManager.Show(NotificationLevel.Info, $"FPS is now limited to: 240 FPS");
                    break;
                case FpsLimitType.Vsync:
                    NotificationManager.Show(NotificationLevel.Info, $"Vsync Enabled");
                    break;
                case FpsLimitType.WaylandVsync:
                    NotificationManager.Show(NotificationLevel.Info,
                        "Wayland VSync is enabled. Note: it only works on Linux under Wayland.");
                    break;
                case FpsLimitType.Custom:
                    NotificationManager.Show(NotificationLevel.Info,
                        $"FPS is now custom limited to: {ConfigManager.CustomFpsLimit.Value}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<FpsLimitType> GetAvailableFpsLimitTypes() =>
            Enum.GetValues(typeof(FpsLimitType))
                .Cast<FpsLimitType>()
                .Where(IsFpsLimitTypeAvailable)
                .ToList();

        /// <summary>
        /// </summary>
        /// <param name="fpsLimitType"></param>
        /// <returns></returns>
        private static bool IsFpsLimitTypeAvailable(FpsLimitType fpsLimitType) =>
            fpsLimitType != FpsLimitType.WaylandVsync || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        ///     Handles when the user holds either Control (CTRL) button and presses O
        /// </summary>
        private void HandleKeyPressCtrlO()
        {
            if (!KeyboardManager.IsCtrlDown())
                return;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.O))
                return;

            if (DialogManager.Dialogs.Count > 0)
                return;

            switch (CurrentScreen.Type)
            {
                case QuaverScreenType.Menu:
                case QuaverScreenType.Select:
                case QuaverScreenType.Editor:
                case QuaverScreenType.Multiplayer:
                case QuaverScreenType.Lobby:
                case QuaverScreenType.Music:
                case QuaverScreenType.Download:
                case QuaverScreenType.Results:
                    DialogManager.Show(new OptionsDialog());
                    break;
            }
        }

        /// <summary>
        ///    Handles when the user holds Control, Shift and Alt, and presses R
        /// </summary>
        private void HandleKeyPressCtrlS()
        {
            // Check for modifier keys
            if (!KeyboardManager.IsCtrlDown())
                return;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.S))
                return;

            // Handle skin reloading
            switch (CurrentScreen.Type)
            {
                case QuaverScreenType.Menu:
                case QuaverScreenType.Select:
                    Transitioner.FadeIn();
                    SkinManager.TimeSkinReloadRequested = GameBase.Game.TimeRunning;
                    break;
            }
        }

        /// <summary>
        ///    Handles when the user holds either Alt (ALT) button and presses Enter
        /// </summary>
        private void HandleKeyPressAltEnter()
        {
            // Don't allow to change to fullscreen when playing
            if (CurrentScreen?.Type == QuaverScreenType.Gameplay)
                return;

            // Check for modifier keys
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt))
                return;

            if (!KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                return;

            ConfigManager.WindowFullScreen.Value = !ConfigManager.WindowFullScreen.Value;
        }

        /// <summary>
        ///     Handles taking screenshots of the game when the user presses F12, and shift to upload.
        /// </summary>
        private void HandleKeyPressScreenshot()
        {
            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyScreenshot.Value))
                return;

            try
            {
                SkinManager.Skin.SoundScreenshot?.CreateChannel()?.Play();

                var w = GraphicsDevice.PresentationParameters.BackBufferWidth;
                var h = GraphicsDevice.PresentationParameters.BackBufferHeight;

                var backBuffer = new int[w * h];

                GraphicsDevice.GetBackBufferData(backBuffer);

                //copy into a texture
                var texture = new Texture2D(GraphicsDevice, w, h, false, GraphicsDevice.PresentationParameters.BackBufferFormat);
                texture.SetData(backBuffer);

                var now = DateTime.Now;

                var path = $"{ConfigManager.ScreenshotDirectory.Value}/{now.Month}{now.Day}{now.Year} {now.Hour}-{now.Minute}-" +
                           $"{now.Second}-{now.Millisecond}.jpg";

                var stream = File.OpenWrite(path);

                texture.SaveAsJpeg(stream, w, h);
                stream.Dispose();

                texture.Dispose();

                NotificationManager.Show(NotificationLevel.Success, $"Screenshot saved. Click here to view!",
                    (sender, args) => Utils.NativeUtils.HighlightInFileManager(path));

                // Upload file to imgur
                if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift))
                    return;

                NotificationManager.Show(NotificationLevel.Info, "Uploading screenshot. Please wait...");

                ThreadScheduler.Run(() =>
                {
                    try
                    {
                        var request = new APIRequestImgurUpload(path);
                        var response = request.ExecuteRequest();

                        if (response is null)
                        {
                            Logger.Error("Failed to upload screenshot to imgur", LogType.Network);
                            NotificationManager.Show(NotificationLevel.Error, "Failed to upload screenshot!");
                        }

                        Clipboard.NativeClipboard.SetText(response);
                        BrowserHelper.OpenURL(response, true);
                        NotificationManager.Show(NotificationLevel.Success, "Successfully uploaded screenshot!");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, LogType.Network);
                        NotificationManager.Show(NotificationLevel.Error, "Failed to upload screenshot!");
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     If true the current screen uses the gameplay music volume rather than the menu music volume.
        /// </summary>
        private bool UsesGameplayMusicVolume => CurrentScreen?.Type == QuaverScreenType.Gameplay
                                               || CurrentScreen?.Type == QuaverScreenType.Editor;

        /// <summary>
        ///     Recalculates the global audio volume muting it if the window is inactive
        ///     and the user has enabled that option. Uses the gameplay music volume while
        ///     a map is actively being played or edited and the menu music volume everywhere else.
        ///
        ///     The music volume is eased towards its new value by <see cref="HandleMusicVolumeFade"/>
        ///     every frame rather than being applied immediately so that switching between
        ///     gameplay/menu music (or muting) doesn't result in a jarring instant volume jump.
        /// </summary>
        /// <param name="immediate">If true, skips the fade and applies the music volume instantly.</param>
        private void UpdateGlobalVolume(bool immediate = false)
        {
            var muted = ConfigManager.MuteAudioOnWindowInactive.Value && !IsActive;
            var musicVolume = UsesGameplayMusicVolume ? ConfigManager.VolumeMusic.Value : ConfigManager.VolumeMenuMusic.Value;
            var target = muted ? 0 : ConfigManager.VolumeGlobal.Value * musicVolume / 100f;

            if (immediate)
            {
                TargetMusicVolume = target;
                AudioTrack.GlobalVolume = target;
            }
            else if (target != TargetMusicVolume)
            {
                MusicVolumeFadeStart = (float) AudioTrack.GlobalVolume;
                MusicVolumeFadeElapsed = 0;
                TargetMusicVolume = target;
            }

            AudioSample.GlobalVolume = muted ? 0 : ConfigManager.VolumeGlobal.Value * ConfigManager.VolumeEffect.Value / 100f;
        }

        /// <summary>
        ///     Smoothly fades music volume between the current and target values over
        ///     <see cref="MusicVolumeFadeTime"/>. The volume is calculated from elapsed
        ///     time instead of incrementally adjusting the current volume, ensuring
        ///     symmetric fade-in/out behavior.
        /// </summary>
        private void HandleMusicVolumeFade(GameTime gameTime)
        {
            if ((float) AudioTrack.GlobalVolume == TargetMusicVolume)
                return;

            MusicVolumeFadeElapsed += gameTime.ElapsedGameTime.TotalMilliseconds;

            var progress = (float) Math.Min(MusicVolumeFadeElapsed / MusicVolumeFadeTime, 1);
            AudioTrack.GlobalVolume = Microsoft.Xna.Framework.MathHelper.Lerp(MusicVolumeFadeStart, TargetMusicVolume, progress);
        }

        /// <summary>
        ///     Handles muting/restoring audio when the window's active state changes and
        ///     switching between gameplay/menu music volume when entering or leaving gameplay.
        /// </summary>
        private void HandleMuteAudioOnWindowInactive()
        {
            var usesGameplayMusicVolume = UsesGameplayMusicVolume;

            if (IsActive == WindowActiveInPreviousFrameForAudio && usesGameplayMusicVolume == WasUsingGameplayMusicVolumeInPreviousFrame)
                return;

            WindowActiveInPreviousFrameForAudio = IsActive;
            WasUsingGameplayMusicVolumeInPreviousFrame = usesGameplayMusicVolume;
            UpdateGlobalVolume();
        }

        /// <summary>
        ///     Handles limiting the game's FPS when the window isn't active.
        /// </summary>
        private void LimitFpsOnInactiveWindow()
        {
            if (!ConfigManager.LowerFpsOnWindowInactive.Value || CurrentScreen != null && CurrentScreen.Exiting)
                return;

            if (PreferCocoaEventLoop())
            {
                InactiveSleepTime = TimeSpan.Zero;
                WindowActiveInPreviousFrame = IsActive;
                return;
            }

            if (!IsActive && WindowActiveInPreviousFrame && OtherGameMapDatabaseCache.OnSyncableScreen() ||
                OtherGameMapDatabaseCache.OnSyncableScreen() && !IsActive && !WindowActiveInPreviousFrame)
            {
                InactiveSleepTime = TimeSpan.FromSeconds(1d / InactiveWindowFpsLimit);
            }
            // Restore user's settings
            else if (!WindowActiveInPreviousFrame && (IsActive || !OtherGameMapDatabaseCache.OnSyncableScreen()))
            {
                InactiveSleepTime = TimeSpan.Zero;
            }

            WindowActiveInPreviousFrame = IsActive;
        }

        /// <summary>
        /// </summary>
        private void UpdateFpsCounterPosition()
        {
            if (Fps == null)
                return;

            Fps.Y = -MenuBorder.HEIGHT - 10;

            if (CurrentScreen?.Type == QuaverScreenType.Editor)
                Fps.Y = -MenuBorder.HEIGHT - 50;
        }

        /// <summary>
        ///     Handles input when opening the online hub
        /// </summary>
        private void HandleOnlineHubInput()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.F8) && !KeyboardManager.IsUniqueKeyPress(Keys.F9))
                return;

            if (CloseOnlineHubDialog())
                return;

            DialogManager.Show(new OnlineHubDialog());
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private bool CloseOnlineHubDialog()
        {
            if (DialogManager.Dialogs.Count == 0)
                return false;

            if (DialogManager.Dialogs.Last().GetType() != typeof(OnlineHubDialog))
                return true;

            var dialog = (OnlineHubDialog)DialogManager.Dialogs.Last();
            dialog?.Close();

            return true;
        }

        /// <summary>
        /// Applies the configured game resolution and updates the virtual screen size
        /// to maintain the correct aspect ratio. If the resolution changes, the current
        /// screen is reloaded to ensure all UI elements and game components are recreated
        /// with the new viewport settings. The game window is then centered on the active
        /// display and the volume controller is recreated.
        /// </summary>
        /// <param name="centerWindow">Whether to recenter the window after applying the change.</param>
        /// <param name="knownOldBackBufferSize">Old backbuffer size to diff against, if not the live value.</param>
        public void ChangeResolution(bool centerWindow = true, Point? knownOldBackBufferSize = null)
        {
            if (!QuaverWindowManager.CanChangeResolutionOnScene)
                return;

            var targetWidth = ConfigManager.WindowWidth.Value;
            var targetHeight = ConfigManager.WindowHeight.Value;

            var oldPos = Window.Position;
            var oldWidth = knownOldBackBufferSize?.X ?? Graphics.PreferredBackBufferWidth;
            var oldHeight = knownOldBackBufferSize?.Y ?? Graphics.PreferredBackBufferHeight;

            if (oldWidth != targetWidth || oldHeight != targetHeight)
            {
                WindowManager.ChangeScreenResolution(new Point(targetWidth, targetHeight));

                var ratio = (float)targetWidth / targetHeight;

                if (ratio >= 16f / 9f)
                    WindowManager.ChangeVirtualScreenSize(new Vector2(WindowManager.BaseResolution.Y * ratio, WindowManager.BaseResolution.Y));
                else
                    WindowManager.ChangeVirtualScreenSize(new Vector2(WindowManager.BaseResolution.X, WindowManager.BaseResolution.X / ratio));
            }

            Graphics.ApplyChanges();

            if (centerWindow)
                CenterWindowOnCurrentMonitor(oldPos, oldWidth, oldHeight, targetWidth, targetHeight);

            if (CurrentScreen == null)
                return;

            switch (CurrentScreen?.Type)
            {
                case QuaverScreenType.Menu:
                    CurrentScreen?.Exit(() => new MainMenuScreen());
                    break;
                case QuaverScreenType.Select:
                    var selectScreen = (SelectionScreen)CurrentScreen;
                    var activeScroll = selectScreen.ActiveScrollContainer.Value;
                    var activePanel = selectScreen.ActiveLeftPanel.Value;
                    CurrentScreen?.Exit(() => new SelectionScreen(activeScroll, activePanel));
                    break;
                case QuaverScreenType.Download:
                    CurrentScreen?.Exit(() => new DownloadingScreen(CurrentScreen.Type));
                    break;
                case QuaverScreenType.Lobby:
                    CurrentScreen?.Exit(() => new MultiplayerLobbyScreen());
                    break;
                case QuaverScreenType.Multiplayer:
                    var screen = (MultiplayerGameScreen)CurrentScreen;
                    screen.DontLeaveGameUponScreenSwitch = true;
                    CurrentScreen?.Exit(() => new MultiplayerGameScreen());
                    break;
                case QuaverScreenType.Music:
                    CurrentScreen?.Exit(() => new MusicPlayerScreen());
                    break;
                case QuaverScreenType.Theatre:
                    CurrentScreen?.Exit(() => new TheaterScreen());
                    break;
            }

            VolumeController?.Destroy();
            VolumeController = new VolumeControl();
        }

        /// <summary>
        ///     The bounds of a monitor, in desktop coordinates.
        /// </summary>
        private struct MonitorBounds
        {
            public int Left, Top, Right, Bottom;
            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        /// <summary>
        ///     Finds the bounds of the monitor containing the given desktop point.
        ///     On Windows, uses Win32 MonitorFromPoint. On macOS/Linux, uses SDL2 display bounds enumeration.
        /// </summary>
        private static MonitorBounds? GetMonitorBoundsAtPoint(int x, int y)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var monitor = MonitorFromPoint(new POINT { X = x, Y = y }, MONITOR_DEFAULTTONEAREST);
                var info = new MONITORINFO { Size = Marshal.SizeOf<MONITORINFO>() };

                if (GetMonitorInfo(monitor, ref info))
                {
                    return new MonitorBounds
                    {
                        Left = info.Monitor.Left,
                        Top = info.Monitor.Top,
                        Right = info.Monitor.Right,
                        Bottom = info.Monitor.Bottom
                    };
                }

                return null;
            }

            try
            {
                var displayCount = SDL_GetNumVideoDisplays();

                for (var i = 0; i < displayCount; i++)
                {
                    if (SDL_GetDisplayBounds(i, out var bounds) != 0)
                        continue;

                    if (x < bounds.x || x >= bounds.x + bounds.w ||
                        y < bounds.y || y >= bounds.y + bounds.h)
                        continue;

                    return new MonitorBounds
                    {
                        Left = bounds.x,
                        Top = bounds.y,
                        Right = bounds.x + bounds.w,
                        Bottom = bounds.y + bounds.h
                    };
                }
            }
            catch (DllNotFoundException)
            {
            }

            return null;
        }

        /// <summary>
        ///     Centers the game window within the given monitor bounds.
        /// </summary>
        private void CenterWindowOnMonitor(MonitorBounds bounds, int targetWidth, int targetHeight)
        {
            var x = bounds.Left + (bounds.Width - targetWidth) / 2;
            var y = bounds.Top + (bounds.Height - targetHeight) / 2;
            Window.Position = new Point(x, y);
        }

        /// <summary>
        ///     Centers the game window on the monitor it was previously on.
        /// </summary>
        /// <param name="oldPos">The window position before the resolution change.</param>
        /// <param name="oldWidth">The window width before the resolution change.</param>
        /// <param name="oldHeight">The window height before the resolution change.</param>
        /// <param name="targetWidth">The new target window width.</param>
        /// <param name="targetHeight">The new target window height.</param>
        private void CenterWindowOnCurrentMonitor(Point oldPos, int oldWidth, int oldHeight, int targetWidth, int targetHeight)
        {
            var centerX = oldPos.X + oldWidth / 2;
            var centerY = oldPos.Y + oldHeight / 2;

            var bounds = GetMonitorBoundsAtPoint(centerX, centerY);

            if (bounds.HasValue)
            {
                CenterWindowOnMonitor(bounds.Value, targetWidth, targetHeight);
                return;
            }

            Window.Position = new Point(centerX - targetWidth / 2, centerY - targetHeight / 2);
        }

        private void OnClientSizeChanged(object sender, EventArgs e)
        {
            // Fullscreen reports the monitor's size here, not a real resize.
            if (Graphics.IsFullScreen)
                return;

            // Capture the pre-resize size once per sequence, before Wobble's handler syncs it.
            if (!_pendingClientSizeChange)
                _pendingOldBackBufferSize = new Point(Graphics.PreferredBackBufferWidth, Graphics.PreferredBackBufferHeight);

            ConfigManager.WindowWidth.Value = Window.ClientBounds.Width;
            ConfigManager.WindowHeight.Value = Window.ClientBounds.Height;

            // Defer to Update() so a multi-frame resize/snap only applies once, after it settles.
            _pendingClientSizeChange = true;
            _lastClientSizeChangeTime = TimeRunning;
        }

        private static void OnAudioOutputDeviceChanged(string deviceName)
        {
            if (ConfigManager.AudioOutputDevice.Value != deviceName)
                ConfigManager.AudioOutputDevice.Value = deviceName;
        }

        public void SetProcessPriority()
        {
            var priority = ProcessPriorityClass.Normal;

            if (ConfigManager.EnableHighProcessPriority.Value)
                priority = ProcessPriorityClass.High;

            try
            {
                using (var p = Process.GetCurrentProcess())
                    p.PriorityClass = priority;
            }
            catch (Win32Exception) { /* do nothing */ }
        }

        public static void SetAudioDevice(bool reloadResources = false)
        {
            for (var i = 1; i < Bass.DeviceCount; i++)
            {
                if (ConfigManager.AudioOutputDevice.Value != Bass.GetDeviceInfo(i).Name)
                    continue;

                AudioManager.Initialize(ConfigManager.DevicePeriod.Value, ConfigManager.DeviceBufferLengthMultiplier.Value, i);
                break;
            }

            if (!reloadResources)
                return;

            AudioEngine.Track?.Stop();
            CustomAudioSampleCache.Dispose();
            SkinManager.Skin.LoadSoundEffects();
        }

#if VISUAL_TESTS
        protected override HotLoaderScreen InitializeHotLoaderScreen() => new HotLoaderScreen(VisualTests);

        private void SetVisualTestingPresence()
        {
            var view = HotLoaderScreen.View as HotLoaderScreenView;

            var screen = view.HotLoader?.Screen;

            if (screen == null)
                return;

            var type = screen?.GetType();

            if (LastVisualTestScreenType == null || LastVisualTestScreenType != type)
            {
                var val = VisualTests.FirstOrDefault(x => x.Value.ToString() == type.ToString()).Key;

                DiscordHelper.Presence.Details = val;
                DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
            }

            LastVisualTestScreenType = type;
        }
#endif
    }
}
