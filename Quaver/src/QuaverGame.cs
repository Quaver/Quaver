﻿using System;
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
using Quaver.Skins;
using Quaver.Tests;
using Quaver.Utility;
using System.Windows.Forms;
using Quaver.Commands;
using Quaver.Discord;
using Quaver.Graphics.Sprite;
#if !PUBLIC
using Quaver.Online;   
#endif
namespace Quaver
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class QuaverGame : Game
    {
        /// <summary>
        ///     The global input manager. For all inputs that are the same across every state.
        /// </summary>
        private GlobalInputManager GlobalInputManager { get; } = new GlobalInputManager();

        public QuaverGame()
        {
            // Set the global graphics device manager & set Window width & height.
            GameBase.GraphicsManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Config.Configuration.WindowWidth,
                PreferredBackBufferHeight = Config.Configuration.WindowHeight,
                IsFullScreen = Config.Configuration.WindowFullScreen,
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
#if !PUBLIC
            // Initialize OnlineEvents
            Rattle.Initialize();
#endif

            // Select a random beatmap if we do in fact have beatmaps.
            if (GameBase.Mapsets.Count != 0) BeatmapUtils.SelectRandomBeatmap();

            // Enable console commands (Only applicable if on debug release)
            CommandHandler.HandleConsoleCommand();

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

            // Load UI .xnb elements
            GameBase.UI.LoadElementsAsContent();

            // Load the Game Skin Before Starting
            Skin.LoadSkin();

            // Initialze the logger
            Logger.Initialize();

            //Initialize Background Manager. Use after Load UI.
            BackgroundManager.Initialize();
            if (GameBase.Mapsets.Count != 0)
            {
                // Load background asynchronously.
                Task.Run(() => BackgroundManager.LoadBackground())
                    .ContinueWith(t => BackgroundManager.Change(GameBase.CurrentBackground));
            }

            // Create Cursor. Use after LoadSkin


            // Set Render Target
            GameBase.GraphicsDevice.SetRenderTarget(GameBase.MainRenderTarget);

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
            GameBase.GameStateManager.ClearStates();
            try
            {
                Bass.Free();
                DiscordRPC.Shutdown();
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of delta time values.</param>
        protected override void Update(GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Check Global Input
            GlobalInputManager.CheckInput();

            // Update Keyboard States
            GameBase.KeyboardState = Keyboard.GetState();
            GameBase.MouseState = Mouse.GetState();

            // Update FpsCounter
            if (Config.Configuration.FpsCounter)
                FpsCounter.Count(dt);

            // Update Background from Background Manager
            BackgroundManager.Update(dt);

            // Update all game states.
            GameBase.GameStateManager.Update(dt);

            // Update Mouse Cursor
            GameBase.Cursor.Update(dt);

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
            GameBase.GraphicsDevice.Clear(Color.White * 0);

            // Draw from Game State Manager
            GameBase.GameStateManager.Draw();

            // Draw Cursor, Logging, and FPS Counter
            GameBase.SpriteBatch.Begin();
            GameBase.Cursor.Draw();
            Logger.Draw(dt);
            if (Config.Configuration.FpsCounter) FpsCounter.Draw();
            GameBase.SpriteBatch.End();

            // Draw Base
            // base.Draw(gameTime);
        }
    }
}
