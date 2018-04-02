using System;
using System.Threading.Tasks;
using System.Timers;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.Database.Beatmaps;
using Quaver.Database;
using Quaver.GameState;
using Quaver.GameState.States;
using Quaver.Graphics;
using Quaver.Input;
using Quaver.Logging;
using Quaver.Modifiers;
using System.Windows.Forms;
using Quaver.Commands;
using Quaver.Discord;
using Quaver.Graphics.UserInterface;
using Quaver.Skinning;
using Steamworks;

namespace Quaver
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

            // Set the global window size
            //GameBase.Window = new Vector4(0, 0, GameBase.GraphicsManager.PreferredBackBufferHeight, GameBase.GraphicsManager.PreferredBackBufferWidth);

            IsFixedTimeStep = false;

            // Use Content in Resources folder (Don't touch this please)
            var resxContent = new ResourceContentManager(Services, Resource1.ResourceManager);
            Content = resxContent;

            // Make a reference to the Window on GameBase
            GameBase.GameWindow = Window;

            // Start watching for directory changes.
            BeatmapImporter.WatchForChanges();
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
           
            //Create new GameStateManager Instance
            GameBase.Content = Content;

            // Load QuaverUserInterface .xnb elements
            GameBase.QuaverUserInterface.LoadElementsAsContent();

            // Load the Game Skin Before Starting
            Skin.LoadSkin();

            // Initialze the logger
            Logger.Initialize();

            //Initialize Background Manager. Use after Load QuaverUserInterface.
            BackgroundManager.Initialize();

            // Select a random beatmap if we do in fact have beatmaps.
            //if (GameBase.Mapsets.Count != 0)
            //    BeatmapHelper.SelectRandomBeatmap();

            // Set Render Target
            GameBase.GraphicsDevice.SetRenderTarget(GameBase.MainRenderTarget);

            // Set up overlay
            GameBase.GameOverlay.Initialize();

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
            Environment.Exit(0);
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
            }
            catch (Exception e)
            {
            }
        }
    }
}
