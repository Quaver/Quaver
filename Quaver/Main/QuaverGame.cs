using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Discord;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Overlays.Volume;
using Quaver.Graphics.UserInterface;
using Quaver.Logging;
using Quaver.Skinning;
using Quaver.States.Menu;
using Quaver.Resources;
using System.IO;
using System.Threading.Tasks;
using Quaver.Database.Scores;

namespace Quaver.Main
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class QuaverGame : Game
    {
        /// <summary>
        ///     Static reference to the current game
        /// </summary>
        public static QuaverGame Game;

        /// <summary>
        ///     Ctor - 
        /// </summary>
        public QuaverGame()
        {
            SetupGame();
            Game = this;

            // Set the global graphics device manager & set Window width & height.
            GameBase.GraphicsManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Config.ConfigManager.WindowWidth.Value,
                PreferredBackBufferHeight = Config.ConfigManager.WindowHeight.Value,
                IsFullScreen = ConfigManager.WindowFullScreen.Value,
                SynchronizeWithVerticalRetrace = false // Turns off vsync
            };
            
            GameBase.GraphicsManager.GraphicsProfile = GraphicsProfile.HiDef;
            GameBase.GraphicsManager.PreferMultiSampling = true;
         
            // TODO: Make thie configurable.
            TargetElapsedTime = TimeSpan.FromMilliseconds(1000 / 240f);
            IsFixedTimeStep = true;
           
            // Use Content in Resources folder (Don't touch this please)
            var resxContent = new ResourceContentManager(Services, QuaverResources.ResourceManager);
            Content = resxContent;

            // Make a reference to the Window on GameBase
            GameBase.GameWindow = Window;

            // Start watching for directory changes.
            MapsetImporter.WatchForChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            DiscordManager.Initialize();
            
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            GameBase.SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Set the global Graphics Device.
            GameBase.GraphicsDevice = GraphicsDevice;
            GameBase.GraphicsDevice.PresentationParameters.MultiSampleCount = 8;
            GameBase.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            GameBase.GraphicsDevice.RasterizerState = new RasterizerState {MultiSampleAntiAlias = true};
            GameBase.GraphicsManager.ApplyChanges();
                       
            //Create new GameStateManager Instance
            GameBase.Content = Content;

            // Load QuaverUserInterface .xnb elements
            GameBase.QuaverUserInterface.LoadElementsAsContent();
            
            // Load all FontAwesome icons
            FontAwesome.Load();
            
            // Load all fonts
            QuaverFonts.Load();

            // Load all titles
            Titles.Load();
            
            // Load the Game Skin 
            GameBase.Skin = new SkinStore();
            
            // Load cursor after skin.
            GameBase.Cursor = new Cursor();
                      
            // Initialze the logger
            Logger.Initialize();

            //Initialize Background Manager. Use after Load QuaverUserInterface.
            BackgroundManager.Initialize();

            // Set Render Target
            GameBase.GraphicsDevice.SetRenderTarget(GameBase.MainRenderTarget);

            // Set up volume controller
            GameBase.VolumeController = new VolumeController();
            GameBase.VolumeController.Initialize(null);
            
            // Set up the navbar
            GameBase.Navbar = new Nav();
            GameBase.Navbar.Initialize(null);
            
            // Change to the loading screen state, where we detect if the song
            // is actually able to be loaded.
            GameBase.GameStateManager.ChangeState(new MainMenuScreen());             
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            GameBase.VolumeController.UnloadContent();
            BackgroundManager.UnloadContent();
            GameBase.GameStateManager.ClearStates();
            GameBase.Navbar.UnloadContent();
            UnloadLibraries();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of delta time values.</param>
        protected override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Needs to be called periodically to dequeue messages according to the lib.
            DiscordManager.Client.Invoke();
            
            // Check Global Input
            GameBase.GlobalInputManager.CheckInput();

            // Update Keyboard States
            GameBase.PreviousKeyboardState = GameBase.KeyboardState;
            GameBase.PreviousMouseState = GameBase.MouseState;
            GameBase.KeyboardState = Keyboard.GetState();
            GameBase.MouseState = Mouse.GetState();

            // Update FpsCounter
            if (ConfigManager.FpsCounter.Value)
                QuaverFpsCounter.Count(dt);

            // Update Background from Background Manager
            BackgroundManager.Update(dt);

            // Update all game states.
            GameBase.GameStateManager.Update(dt);

            // Update Mouse QuaverCursor
            GameBase.Cursor.Update(dt);

            // Update volume controller
            GameBase.VolumeController.Update(dt);
            
            // Update Navbar
            GameBase.Navbar.Update(dt);
            
            // Run scheduled background tasks
            if (GameBase.GameTime.ElapsedMilliseconds - CommonTaskScheduler.LastRunTime >= 5000)
                CommonTaskScheduler.Run();
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Clear Background so it doesnt render everything from previous frame
            GameBase.GraphicsDevice.Clear(Color.Black);

            // Draw from Game State Manager
            GameBase.GameStateManager.Draw();

            // Draw QuaverCursor, Logging, and FPS Counter
            GameBase.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, GraphicsDevice.RasterizerState);
            GameBase.VolumeController.Draw();
            GameBase.Navbar.Draw();
            GameBase.Cursor.Draw();
            Logger.Draw(dt);
     
            if (ConfigManager.FpsCounter.Value)
                QuaverFpsCounter.Draw();

            GameBase.SpriteBatch.End();

            // Draw Base
            // base.Draw(gameTime);
        }

        /// <summary>
        ///     Quits the game
        /// </summary>
        internal static void Quit()
        {
            UnloadLibraries();
            Game.Exit();
        }

        /// <summary>
        ///     Unloads all third-party libraries such as BASS, Discord RPC, and Steam    
        /// </summary>
        internal static void UnloadLibraries()
        {
            try
            {
                GameBase.AudioEngine.Free();
                DiscordManager.Client.Dispose();
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        ///     This method changes the window to match configuration settings
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="fullscreen"></param>
        /// <param name="letterbox"></param>
        public static void ChangeWindow(bool fullscreen, bool letterbox, Point? resolution = null)
        {
            // Change Resolution
            if (resolution != null)
            {
                ConfigManager.WindowWidth.Value = resolution.Value.X;
                ConfigManager.WindowHeight.Value = resolution.Value.Y;
                GameBase.GraphicsManager.PreferredBackBufferWidth = resolution.Value.X;
                GameBase.GraphicsManager.PreferredBackBufferHeight = resolution.Value.Y;
                GameBase.WindowRectangle = new DrawRectangle(0, 0, resolution.Value.X, resolution.Value.Y);
            }

            // Update Fullscreen
            if (fullscreen != GameBase.GraphicsManager.IsFullScreen)
                GameBase.GraphicsManager.IsFullScreen = fullscreen;

            // Update letter boxing
            if (letterbox)
            {
                //do stuff
            }

            // Apply changes to graphics manager
            GameBase.GraphicsManager.ApplyChanges();

            // Log this event
            Logger.LogImportant("Window Settings Changed!", LogType.Runtime);
            Logger.LogImportant($"Res: {GameBase.GraphicsManager.PreferredBackBufferWidth}x {GameBase.GraphicsManager.PreferredBackBufferHeight}", LogType.Runtime);
            Logger.LogImportant($"Letterboxing: {letterbox}", LogType.Runtime);
            Logger.LogImportant($"FullScreen: {GameBase.GraphicsManager.IsFullScreen}", LogType.Runtime);
        }

        private void SetupGame()
        {
            // Initialize Config
            ConfigManager.InitializeConfig();

            // Delete Temp Files
            DeleteTemporaryFiles();

            // Set up the game
            Setup();
        }

        /// <summary>
        ///     Deletes all temporary files if there are any.
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
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Responsible for initializing and setting the map database and setting the loaded maps
        /// </summary>
        private static void Setup()
        {
            // Create now playing folder
            Directory.CreateDirectory(ConfigManager.DataDirectory + "/temp/Now Playing/");

            // Set the build version
            GameBase.BuildVersion = MapsetHelper.GetMd5Checksum(ConfigManager.GameDirectory + "/" + "Quaver.exe");

            // After initializing the configuration, we want to sync the map database, and load the dictionary of mapsets.
            var loadGame = Task.Run(async () =>
            {
                await MapCache.LoadAndSetMapsets();

                // Create the local scores database if it doesn't already exist
                await LocalScoreCache.CreateScoresDatabase();

                // Force garbage collection
                GC.Collect();
            });
            Task.WaitAll(loadGame);
        }
    }
}
