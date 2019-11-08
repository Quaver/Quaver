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
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Database.Settings;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Volume;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Alpha;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Settings;
using Quaver.Shared.Screens.Tests.Footer;
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
        /// <inheritdoc />
        /// <summary>
        /// </summary>
    protected override bool IsReadyToUpdate { get; set; }

        /// <summary>
        ///     The volume controller for the game.
        /// </summary>
        public VolumeController VolumeController { get; private set; }

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

#if VISUAL_TESTS
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

            WindowManager.ChangeVirtualScreenSize(new Vector2(1366, 768));
            WindowManager.ChangeScreenResolution(new Point(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value));

            // Full-screen
            Graphics.IsFullScreen = ConfigManager.WindowFullScreen.Value;

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
            Fonts.Load();

            BackgroundHelper.Initialize();

            // Load the user's skin
            SkinManager.Load();

            // Create the global FPS counter.
            CreateFpsCounter();
            VolumeController = new VolumeController() {Parent = GlobalUserInterface};
            BackgroundManager.Initialize();
            Transitioner.Initialize();

            // Make the cursor appear over the volume controller.
            ListHelper.Swap(GlobalUserInterface.Children, GlobalUserInterface.Children.IndexOf(GlobalUserInterface.Cursor),
                                                            GlobalUserInterface.Children.IndexOf(VolumeController));

            IsReadyToUpdate = true;

            Logger.Debug($"Currently running Quaver version: `{Version}`", LogType.Runtime);

#if VISUAL_TESTS
            Window.Title = $"Quaver Visual Test Runner";
#else
            Window.Title = !IsDeployedBuild ? $"Quaver - {Version}" : $"Quaver v{Version}";
            QuaverScreenManager.ScheduleScreenChange(() => new MenuScreen());
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

            // Run scheduled background tasks
            CommonTaskScheduler.Run();

            BackgroundManager.Update(gameTime);
            BackgroundHelper.Update(gameTime);
            ChatManager.Update(gameTime);
            DialogManager.Update(gameTime);

            HandleGlobalInput(gameTime);

            QuaverScreenManager.Update(gameTime);
            NotificationManager.Update(gameTime);
            Transitioner.Update(gameTime);

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

            // Draw the global container last.
            GlobalUserInterface.Draw(gameTime);

            Transitioner.Draw(gameTime);
        }

        /// <summary>
        ///     Performs any initial setup the game needs to run.
        /// </summary>
        private void PerformGameSetup()
        {
            ConfigManager.Initialize();

            DeleteTemporaryFiles();

            ScoreDatabaseCache.CreateTable();
            MapDatabaseCache.Load(false);
            QuaverSettingsDatabaseCache.Initialize();
            JudgementWindowsDatabaseCache.Load();

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

            // Handle discord rich presence.
            DiscordHelper.Initialize("376180410490552320");
            DiscordHelper.Presence = new DiscordRpc.RichPresence()
            {
                LargeImageKey = "quaver",
                LargeImageText = ConfigManager.Username.Value,
                EndTimestamp = 0
            };

            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            // Create bindable for selected map.
            if (MapManager.Mapsets.Count != 0)
                MapManager.Selected = new Bindable<Map>(MapManager.Mapsets.First().Maps.First());
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
                TextFps =
                {
                    Tint = Color.White
                },
                X = -5,
                Y = -36,
                Alpha = 0
            };

            ShowFpsCounter(fpsCounter);
            ConfigManager.FpsCounter.ValueChanged += (o, e) => ShowFpsCounter(fpsCounter);
        }

        /// <summary>
        ///     Shows the FPs counter based on the current config variable.
        /// </summary>
        private static void ShowFpsCounter(FpsCounter counter) => counter.TextFps.Alpha = ConfigManager.FpsCounter.Value ? 1 : 0;

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
                    break;
                case FpsLimitType.Limited:
                    Graphics.SynchronizeWithVerticalRetrace = false;
                    IsFixedTimeStep = true;
                    TargetElapsedTime = TimeSpan.FromSeconds(1d / 240d);
                    break;
                case FpsLimitType.Vsync:
                    Graphics.SynchronizeWithVerticalRetrace = true;
                    IsFixedTimeStep = true;
                    break;
                case FpsLimitType.Custom:
                    Graphics.SynchronizeWithVerticalRetrace = false;
                    TargetElapsedTime = TimeSpan.FromSeconds(1d / ConfigManager.CustomFpsLimit.Value);
                    IsFixedTimeStep = true;
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
                    DialogManager.Show(new SettingsDialog());
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
        ///     Handles limiting the game's FPS when the window isn't active.
        /// </summary>
        private void LimitFpsOnInactiveWindow()
        {
            if (CurrentScreen != null && CurrentScreen.Exiting)
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

#if VISUAL_TESTS
        protected override HotLoaderScreen InitializeHotLoaderScreen() => new HotLoaderScreen(new Dictionary<string, Type>()
        {
            {"Menu Footer", typeof(MenuFooterTestScreen)}
        });
#endif
    }
}
