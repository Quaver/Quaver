/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Quaver.Server.Common.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Chatting;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Graphics.Overlays.Volume;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Imgur;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Alpha;
using Quaver.Shared.Screens.Downloading;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Options;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Settings;
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
using Quaver.Shared.Screens.Tests.Volume;
using Quaver.Shared.Skinning;
using Steamworks;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Discord;
using Wobble.Discord.RPC;
using Wobble.Extended.HotReload;
using Wobble.Extended.HotReload.Screens;
using Wobble.Graphics;
using Wobble.Graphics.UI.Debugging;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.IO;
using Wobble.Logging;
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
        /// <summary>
        ///     Scaling factor for skin values and scroll speed to convert them to the UI redesign coordinate system.
        /// </summary>
        public const float SkinScalingFactor = 1920f / 1366;

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
                return $@"{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build}";
            }
        }

        /// <summary>
        ///     Used to detect when to limit FPS if the user's window isn't active.
        /// </summary>
        private bool WindowActiveInPreviousFrame { get; set; }

        /// <summary>
        ///     Sometimes we'd like to perform actions on the first update, such as
        ///     creating <see cref="OnlineHub"/>
        /// </summary>
        public bool FirstUpdateCalled { get; set; }

#if VISUAL_TESTS
        /// <summary>
        ///     The visual screen type in the previous frame
        /// </summary>
        private Type LastVisualTestScreenType { get; set; }

        /// <summary>
        /// </summary>
        private Dictionary<string, Type> VisualTests { get; } = new Dictionary<string, Type>()
        {
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
            {"Main Menu", typeof(MainMenuScreen)},
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
        };

        public QuaverGame(HotLoader hl) : base(hl)
#else
        public QuaverGame()
#endif
        {
            Content.RootDirectory = "Content";
            InitializeFpsLimiting();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            PerformGameSetup();

            WindowManager.ChangeVirtualScreenSize(new Vector2(1920, 1080));
            WindowManager.ChangeScreenResolution(new Point(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value));

            // Full-screen
            Graphics.IsFullScreen = ConfigManager.WindowFullScreen.Value;
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Window.IsBorderless = ConfigManager.WindowBorderless.Value;

            // Apply all graphics changes
            Graphics.ApplyChanges();

            // Handle file dropped event.
            Window.FileDropped += MapsetImporter.OnFileDropped;

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

            Resources.AddStore(new DllResourceStore("Quaver.Resources.dll"));
            SteamManager.SendAvatarRetrievalRequest(SteamUser.GetSteamID().m_SteamID);

            // Load all game assets.
            Fonts.LoadGdiFonts();
            Fonts.LoadWobbleFonts();

            BackgroundHelper.Initialize();

            // Load the user's skin
            SkinManager.Load();

            // Create the global FPS counter.
            CreateFpsCounter();
            BackgroundManager.Initialize();
            Transitioner.Initialize();

            IsReadyToUpdate = true;

            Logger.Debug($"Currently running Quaver version: `{Version}`", LogType.Runtime);

#if VISUAL_TESTS
            Window.Title = $"Quaver Visual Test Runner";
#else
            Window.Title = !IsDeployedBuild ? $"Quaver - {Version}" : $"Quaver v{Version}";
            QuaverScreenManager.ScheduleScreenChange(() => new MainMenuScreen());
#endif
        }

        /// <inheritdoc />
        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            OnlineManager.Client?.Disconnect();
            Transitioner.Dispose();
            DiscordHelper.Shutdown();
            base.UnloadContent();
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
                // Create the online hub on the first update, since it uses text, and we have to wait for things to
                // be initialized
                OnlineHub = new OnlineHub();
                OnlineChat = new OnlineChat();
                VolumeController = new VolumeControl();

                FirstUpdateCalled = true;
            }

            // Run scheduled background tasks
            CommonTaskScheduler.Run();

            BackgroundManager.Update(gameTime);
            BackgroundHelper.Update(gameTime);
            DialogManager.Update(gameTime);

            HandleGlobalInput(gameTime);
            HandleOnlineHubInput();

            QuaverScreenManager.Update(gameTime);
            NotificationManager.Update(gameTime);
            VolumeController.Update(gameTime);
            Transitioner.Update(gameTime);

#if VISUAL_TESTS
            SetVisualTestingPresence();
#endif

            SkinManager.HandleSkinReloading();
            LimitFpsOnInactiveWindow();
        }

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
            VolumeController.Draw(gameTime);
            GlobalUserInterface.Draw(gameTime);

            Transitioner.Draw(gameTime);

            ClearAlphaChannel(gameTime);
        }

        /// <summary>
        ///     Performs any initial setup the game needs to run.
        /// </summary>
        private void PerformGameSetup()
        {
            ConfigManager.Initialize();

            DeleteTemporaryFiles();

            DatabaseManager.Initialize();
            ScoreDatabaseCache.CreateTable();
            MapDatabaseCache.Load(false);
            QuaverSettingsDatabaseCache.Initialize();
            JudgementWindowsDatabaseCache.Load();
            UserProfileDatabaseCache.Load();

            // Force garabge collection.
            GC.Collect();

            // Start watching for mapset changes in the folder.
            MapsetImporter.WatchForChanges();

            // Initially set the global volume.
            AudioTrack.GlobalVolume = ConfigManager.VolumeGlobal.Value * ConfigManager.VolumeMusic.Value / 100f;
            AudioSample.GlobalVolume = ConfigManager.VolumeGlobal.Value * ConfigManager.VolumeEffect.Value / 100f;

            ConfigManager.VolumeGlobal.ValueChanged += (sender, e) =>
            {
                AudioTrack.GlobalVolume = e.Value * ConfigManager.VolumeMusic.Value / 100f;
                AudioSample.GlobalVolume = e.Value * ConfigManager.VolumeEffect.Value / 100f;
            };
            ConfigManager.VolumeMusic.ValueChanged += (sender, e) => AudioTrack.GlobalVolume = ConfigManager.VolumeGlobal.Value * e.Value / 100f;
            ConfigManager.VolumeEffect.ValueChanged += (sender, e) => AudioSample.GlobalVolume = ConfigManager.VolumeEffect.Value * e.Value / 100f;

            ConfigManager.Pitched.ValueChanged += (sender, e) => AudioEngine.Track.ApplyRate(e.Value);
            ConfigManager.FpsLimiterType.ValueChanged += (sender, e) => InitializeFpsLimiting();
            ConfigManager.WindowFullScreen.ValueChanged += (sender, e) => Graphics.IsFullScreen = e.Value;
            ConfigManager.WindowBorderless.ValueChanged += (sender, e) => Window.IsBorderless = e.Value;
            ConfigManager.SelectedGameMode.ValueChanged += (sender, args) =>
            {
                DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(args.Value);
                DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
                DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);
                DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
            };

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
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            // Create bindable for selected map.
            if (MapManager.Mapsets.Count != 0)
                MapManager.Selected = new Bindable<Map>(MapManager.Mapsets.First().Maps.First());

            MapManager.Selected.ValueChanged += (sender, args) =>
            {
                if (MapManager.RecentlyPlayed.Contains(args.Value))
                    MapManager.RecentlyPlayed.Remove(args.Value);

                MapManager.RecentlyPlayed.Add(args.Value);
            };
        }

        /// <summary>
        ///     Deletes all of the temporary files for the game if they exist.
        /// </summary>
        private static void DeleteTemporaryFiles()
        {
            try
            {
                foreach (var file in new DirectoryInfo(ConfigManager.DataDirectory + "/temp/").GetFiles("*", SearchOption.AllDirectories))
                    file.Delete();

                foreach (var dir in new DirectoryInfo(ConfigManager.DataDirectory + "/temp/").GetDirectories("*", SearchOption.AllDirectories))
                    dir.Delete(true);
            }
            catch (Exception)
            {
                // ignored
            }

            // Create a directory that displays the "Now playing" song.
            Directory.CreateDirectory($"{ConfigManager.DataDirectory}/temp/Now Playing");
        }

        /// <summary>
        ///     Creates the FPS counter to display on a global state.
        /// </summary>
        private void CreateFpsCounter()
        {
            var fpsCounter = new FpsCounter(FontsBitmap.GothamRegular, 18)
            {
                Parent = GlobalUserInterface,
                Alignment = Alignment.BotRight,
                Size = new ScalableVector2(70, 30),
                X = -5,
                Y = -36,
                Visible = false
            };

            ShowFpsCounter(fpsCounter);
            ConfigManager.FpsCounter.ValueChanged += (o, e) => ShowFpsCounter(fpsCounter);
        }

        /// <summary>
        ///     Shows the FPS counter based on the current config variable.
        /// </summary>
        private static void ShowFpsCounter(FpsCounter counter) => counter.Visible = ConfigManager.FpsCounter.Value;

        /// <summary>
        ///    Handles limiting/unlimiting FPS based on user config
        /// </summary>
        public void InitializeFpsLimiting()
        {
            switch (ConfigManager.FpsLimiterType.Value)
            {
                case FpsLimitType.Unlimited:
                    Graphics.SynchronizeWithVerticalRetrace = false;
                    IsFixedTimeStep = false;
                    WaylandVsync = false;
                    break;
                case FpsLimitType.Limited:
                    Graphics.SynchronizeWithVerticalRetrace = false;
                    IsFixedTimeStep = true;
                    TargetElapsedTime = TimeSpan.FromSeconds(1d / 240d);
                    WaylandVsync = false;
                    break;
                case FpsLimitType.Vsync:
                    Graphics.SynchronizeWithVerticalRetrace = true;
                    IsFixedTimeStep = true;
                    WaylandVsync = false;
                    break;
                case FpsLimitType.WaylandVsync:
                    Graphics.SynchronizeWithVerticalRetrace = false;
                    IsFixedTimeStep = false;
                    WaylandVsync = true;
                    break;
                case FpsLimitType.Custom:
                    Graphics.SynchronizeWithVerticalRetrace = false;
                    TargetElapsedTime = TimeSpan.FromSeconds(1d / ConfigManager.CustomFpsLimit.Value);
                    IsFixedTimeStep = true;
                    WaylandVsync = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Graphics.ApplyChanges();
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

            var index = (int) ConfigManager.FpsLimiterType.Value;

            if (index + 1 < Enum.GetNames(typeof(FpsLimitType)).Length)
                ConfigManager.FpsLimiterType.Value = (FpsLimitType) index + 1;
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
        ///     Handles when the user holds either Control (CTRL) button and presses O
        /// </summary>
        private void HandleKeyPressCtrlO()
        {
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) &&
                !KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
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
            if (!(KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl)))
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

                NotificationManager.Show(NotificationLevel.Success, $"Screenshot saved. Click here to view!" ,
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

                        if (response == null)
                            throw new Exception("Failed to upload screenshot to imgur");

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
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Handles limiting the game's FPS when the window isn't active.
        /// </summary>
        private void LimitFpsOnInactiveWindow()
        {
            if (!ConfigManager.LowerFpsOnWindowInactive.Value || CurrentScreen != null && CurrentScreen.Exiting)
                return;

            if (!IsActive && WindowActiveInPreviousFrame && OtherGameMapDatabaseCache.OnSyncableScreen() ||
                OtherGameMapDatabaseCache.OnSyncableScreen() && !IsActive && !WindowActiveInPreviousFrame)
            {
                InactiveSleepTime = TimeSpan.FromSeconds(1d / 30);
            }
            // Restore user's settings
            else if (!WindowActiveInPreviousFrame && (IsActive || !OtherGameMapDatabaseCache.OnSyncableScreen()))
            {
                InactiveSleepTime = TimeSpan.Zero;
                InitializeFpsLimiting();
            }

            WindowActiveInPreviousFrame = IsActive;
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

            var dialog = (OnlineHubDialog) DialogManager.Dialogs.Last();
            dialog?.Close();

            return true;
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
