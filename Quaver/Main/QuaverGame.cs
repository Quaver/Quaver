using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Discord;
using Steamworks;
using Quaver.Graphics.Base;
using Quaver.Graphics.UserInterface;
using Quaver.Logging;
using Quaver.Online;
using Quaver.Skinning;
using Quaver.States.Menu;
using Quaver.Resources;

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

        public QuaverGame()
        {
            Game = this;

            // Set the global graphics device manager & set Window width & height.
            GameBase.GraphicsManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Config.ConfigManager.WindowWidth,
                PreferredBackBufferHeight = Config.ConfigManager.WindowHeight,
                IsFullScreen = Config.ConfigManager.WindowFullScreen,
                SynchronizeWithVerticalRetrace = false // Turns off vsync
            };

            GameBase.GraphicsManager.GraphicsProfile = GraphicsProfile.HiDef;
            GameBase.GraphicsManager.PreferMultiSampling = true;
            
            // Set the global window size
            //GameBase.Window = new Vector4(0, 0, GameBase.GraphicsManager.PreferredBackBufferHeight, GameBase.GraphicsManager.PreferredBackBufferWidth);

            IsFixedTimeStep = false;

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
            // Handle Text Input
            //GameBase.GameWindow.TextInput += TextEndered;

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
            GameBase.GraphicsManager.ApplyChanges();
           
            //Create new GameStateManager Instance
            GameBase.Content = Content;

            // Load QuaverUserInterface .xnb elements
            GameBase.QuaverUserInterface.LoadElementsAsContent();

            // Load all FontAwesome icons
            FontAwesome.Load();
            
            // Load all fonts
            QuaverFonts.Load();

            // Load the Game Skin Before Starting
            Skin.LoadSkin();

            // Initialze the logger
            Logger.Initialize();

            //Initialize Background Manager. Use after Load QuaverUserInterface.
            BackgroundManager.Initialize();

            // Set Render Target
            GameBase.GraphicsDevice.SetRenderTarget(GameBase.MainRenderTarget);

            // Set up overlay
            GameBase.GameOverlay.Initialize();

            SteamworksHelper.Initialize();

            // Change to the loading screen state, where we detect if the song
            // is actually able to be loaded.
            GameBase.GameStateManager.ChangeState(new MainMenuState());             
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            BackgroundManager.UnloadContent();
            GameBase.GameOverlay.UnloadContent();
            GameBase.GameStateManager.ClearStates();
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

            // Check Global Input
            GameBase.GlobalInputManager.CheckInput();

            // Update Keyboard States
            GameBase.KeyboardState = Keyboard.GetState();
            GameBase.MouseState = Mouse.GetState();

            // Update FpsCounter
            if (Config.ConfigManager.FpsCounter)
                QuaverFpsCounter.Count(dt);

            // Update Background from Background Manager
            BackgroundManager.Update(dt);
            GameBase.GameOverlay.Update(dt);

            // Update all game states.
            GameBase.GameStateManager.Update(dt);

            // Update Mouse Cursor
            GameBase.QuaverCursor.Update(dt);

            // Run Steam callbacks every frame to frequently stay updated with the API
            if (SteamworksHelper.IsInitialized)
                SteamAPI.RunCallbacks();

            // Update Mouse QuaverCursor
            GameBase.QuaverCursor.Update(dt);

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
            GameBase.GraphicsDevice.Clear(Color.Transparent);

            // Draw from Game State Manager
            GameBase.GameStateManager.Draw();

            // Draw QuaverCursor, Logging, and FPS Counter
            GameBase.SpriteBatch.Begin();
            GameBase.GameOverlay.Draw();
            GameBase.QuaverCursor.Draw();

            Logger.Draw(dt);

            if (Config.ConfigManager.FpsCounter)
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
                DiscordRPC.Shutdown();

                if (SteamworksHelper.IsInitialized)
                    SteamAPI.Shutdown();
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
                ConfigManager.WindowWidth = resolution.Value.X;
                ConfigManager.WindowHeight = resolution.Value.Y;
                GameBase.GraphicsManager.PreferredBackBufferWidth = resolution.Value.X;
                GameBase.GraphicsManager.PreferredBackBufferHeight = resolution.Value.Y;
                GameBase.WindowRectangle = new DrawRectangle(0, 0, resolution.Value.X, resolution.Value.Y);
                GameBase.WindowUIScale = GameBase.WindowRectangle.Height / GameBase.ReferenceResolution.Y;
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
    }
}
