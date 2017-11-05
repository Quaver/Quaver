using System;
using System.Threading.Tasks;
using System.Timers;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.Beatmaps;
using Quaver.Database;
using Quaver.GameState;
using Quaver.Gameplay;
using Quaver.GameState.States;
using Quaver.Graphics;
using Quaver.Input;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.Skins;
using Quaver.Tests;
using Quaver.Utility;
using System.Windows.Forms;

namespace Quaver.Main
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
                SynchronizeWithVerticalRetrace = false // Turns off vsync
            };

            // Set the global window size
            GameBase.Window = new Rectangle(0, 0, GameBase.GraphicsManager.PreferredBackBufferWidth, GameBase.GraphicsManager.PreferredBackBufferHeight);

            IsFixedTimeStep = false;

            // Use Content in Resources folder (Don't touch this please)
            var resxContent = new ResourceContentManager(Services, Resource1.ResourceManager);
            Content = resxContent;

            // Start watching for directory changes.
            BeatmapImporter.WatchForChanges();

            // Creates an audio timer that runs throughout the existence of this game instance
            // and automagically frees up all available audio streams to prevent memory leaks.
            // this was the best way i could figure out.
            Task.Run(() =>
            {
                var audioTimer = new System.Timers.Timer();
                audioTimer.Elapsed += AudioTimerHandler;
                audioTimer.Interval = 2000;
                audioTimer.Enabled = true;
            });
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        { 
            // Select a random beatmap if we do in fact have beatmaps.
            if (GameBase.Beatmaps.Count != 0)
                BeatmapUtils.SelectRandomBeatmap();

            // Add some mods
            // ModManager.AddMod(ModIdentifier.Speed, 1.5f);
            // ModManager.AddMod(ModIdentifer.NoSliderVelocities);
            // ModManager.RemoveMod(ModIdentifier.Speed);
            // ModManager.RemoveAllMods();

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

            // Load the Game Skin Before Starting
            GameBase.LoadSkin();

            // Create Cursor. Use after LoadSkin
            GameBase.LoadCursor();

            // Change to the loading screen state, where we detect if the song
            // is actually able to be loaded.
            GameStateManager.Instance.ChangeState(new MainMenuState());             
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            GameStateManager.Instance.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of delta time values.</param>
        protected override void Update(GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Update FpsCounter
            if (Config.Configuration.FpsCounter)
                FpsCounter.Count(dt);

            // Update all game states.
            GameStateManager.Instance.Update(gameTime);

            GameBase.KeyboardState = Keyboard.GetState();
            GameBase.MouseState = Mouse.GetState();
            GameBase.Cursor.Update(dt);

            // Check Global Input
            GlobalInputManager.CheckInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            double dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Start SriteBatch
            GameBase.SpriteBatch.Begin();

            // Set Background Color
            GameBase.GraphicsDevice.Clear(Color.DarkSlateGray);

            // Draw the game states
            GameStateManager.Instance.Draw();
            
            // Draw the FPS Counter
            if (Config.Configuration.FpsCounter)
                FpsCounter.Draw();

            //Draw log manager logs
            LogManager.Draw(dt);

            //Draw cursor
            GameBase.Cursor.Draw();

            // Draw everything else in the base class
            base.Draw(gameTime);
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Frees all available audio streams every x seconds.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void AudioTimerHandler(object source, ElapsedEventArgs e)
        {
            Task.Run(() => AudioHandler.FreeAvailableStreams());
        }
    }
}
