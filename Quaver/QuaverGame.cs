using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Graphics.Notifications;
using Quaver.Graphics.Overlays.Volume;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Scheduling;
using Quaver.Screens.Connecting;
using Quaver.Screens.Menu;
using Quaver.Screens.Splash;
using Quaver.Shaders;
using Quaver.Skinning;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Discord;
using Wobble.Discord.RPC;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.UI.Debugging;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver
{
    public class QuaverGame : WobbleGame
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override bool IsReadyToUpdate { get; set; }

        /// <summary>
        ///     The volume controller for the game.
        /// </summary>
        public VolumeController VolumeController { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public QuaverGame()
        {
            IsFixedTimeStep = false;
            Graphics.SynchronizeWithVerticalRetrace = false;
            Graphics.ApplyChanges();

            Graphics.PreparingDeviceSettings += (sender, args) =>
            {
                Graphics.GraphicsProfile = GraphicsProfile.HiDef;
                Graphics.PreferMultiSampling = true;
                args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
            };
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

            // Load all game assets.
            FontAwesome.Load();
            Fonts.Load();
            Titles.Load();
            UserInterface.Load();

            // Load the user's skin
            SkinManager.Load();

            // Initialize the logger now that we have fonts loaded
            Logger.Initialize();

            // Create the global FPS counter.
            CreateFpsCounter();
            VolumeController = new VolumeController() {Parent = GlobalUserInterface};
            BackgroundManager.Initialize();

            // Make the cursor appear over the volume controller.
            ListHelper.Swap(GlobalUserInterface.Children, GlobalUserInterface.Children.IndexOf(GlobalUserInterface.Cursor),
                                                            GlobalUserInterface.Children.IndexOf(VolumeController));

            IsReadyToUpdate = true;
            ScreenManager.ChangeScreen(new SplashScreen());
        }

        /// <inheritdoc />
        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
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

            // Run scheduled background tasks
            CommonTaskScheduler.Run();
            BackgroundManager.Update(gameTime);
            NotificationManager.Update(gameTime);
            DialogManager.Update(gameTime);
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

            GameBase.DefaultSpriteBatchOptions.Begin();
            Logger.Draw(gameTime);
            SpriteBatch.End();

            NotificationManager.Draw(gameTime);

            // Draw dialogs
            DialogManager.Draw(gameTime);

            // Draw the global container last.
            GlobalUserInterface.Draw(gameTime);

        }

        /// <summary>
        ///     Performs any initial setup the game needs to run.
        /// </summary>
        private static void PerformGameSetup()
        {
            ConfigManager.Initialize();
            DeleteTemporaryFiles();

            LocalScoreCache.CreateTable();
            MapCache.LoadAndSetMapsets();

            // Force garabge collection.
            GC.Collect();

            // Start watching for mapset changes in the folder.
            MapsetImporter.WatchForChanges();

            // Initially set the global volume.
            AudioTrack.GlobalVolume = ConfigManager.VolumeGlobal.Value;
            AudioSample.GlobalVolume = ConfigManager.VolumeEffect.Value;

            // Change master volume whenever it changes.
            ConfigManager.VolumeGlobal.ValueChanged += (sender, e) =>
            {
                AudioTrack.GlobalVolume = e.Value;
                AudioSample.GlobalVolume = e.Value;
            };

            // Change track volume whenever it changed
            ConfigManager.VolumeMusic.ValueChanged += (sender, e) =>
            {
                if (AudioEngine.Track != null)
                    AudioEngine.Track.Volume = e.Value;
            };

            ConfigManager.VolumeEffect.ValueChanged += (sender, e) => AudioSample.GlobalVolume = e.Value;
            ConfigManager.Pitched.ValueChanged += (sender, e) => AudioEngine.Track.ToggleRatePitching(e.Value);

            DiscordManager.CreateClient("376180410490552320");
            DiscordManager.Client.SetPresence(new RichPresence()
            {
                Assets = new Wobble.Discord.RPC.Assets()
                {
                    LargeImageKey = "quaver",
                    LargeImageText = ConfigManager.Username.Value
                },
                Timestamps = new Timestamps()
            });

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
            var fpsCounter = new FpsCounter(Fonts.AllerBold16, 0.80f)
            {
                Parent = GlobalUserInterface,
                Alignment = Alignment.BotRight,
                Size = new ScalableVector2(70, 30),
                TextFps =
                {
                    TextColor = Color.LimeGreen
                },
                X = -10,
                Y = -10,
                Alpha = 0
            };

            ShowFpsCounter(fpsCounter);
            ConfigManager.FpsCounter.ValueChanged += (o, e) => ShowFpsCounter(fpsCounter);
        }

        /// <summary>
        ///     Shows the FPs counter based on the current config variable.
        /// </summary>
        private static void ShowFpsCounter(FpsCounter counter) => counter.TextFps.Alpha = ConfigManager.FpsCounter.Value ? 1 : 0;
    }
}
