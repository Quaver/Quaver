using System;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Beatmaps;
using Quaver.Database;
using Quaver.GameState;
using Quaver.Gameplay;
using Quaver.Graphics;
using Quaver.Input;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.Skins;
using Quaver.Tests;
using Quaver.Utility;

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
            // Set the global graphics device manager.
            GameBase.GraphicsManager = new GraphicsDeviceManager(this);

            // Set the global window size
            GameBase.Window = new Rectangle(0, 0, GameBase.GraphicsManager.PreferredBackBufferWidth, GameBase.GraphicsManager.PreferredBackBufferHeight);

            // Turn off vsync
            GameBase.GraphicsManager.SynchronizeWithVerticalRetrace = false; 
            IsFixedTimeStep = false;

            // Use Content in Resources folder (Don't touch this please)
            var resxContent = new ResourceContentManager(Services, Resource1.ResourceManager);
            Content = resxContent;

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

            // Change to the play screen state (Currently utilized for testing.)
            GameStateManager.Instance.ChangeState(new StatePlayScreen());                
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
            // Update FpsCounter
            if (Config.Configuration.FpsCounter)
                FpsCounter.Count(gameTime.ElapsedGameTime.TotalSeconds);

            // Update all game states.
            GameStateManager.Instance.Update(gameTime);

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
            // Start SriteBatch
            GameBase.SpriteBatch.Begin();

            // Set Background Color
            GameBase.GraphicsDevice.Clear(Color.DarkSlateGray);

            // Draw the game states
            GameStateManager.Instance.Draw();
            
            // Draw the FPS Counter
            if (Config.Configuration.FpsCounter)
                FpsCounter.Draw();

            SpriteManager.Draw();
            LogTracker.Draw(gameTime.ElapsedGameTime.TotalSeconds);

            // Draw everything else in the base class
            base.Draw(gameTime);
            GameBase.SpriteBatch.End();
        }
    }
}
