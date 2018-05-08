using System;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics.Sprites;
using Quaver.Input;
using Quaver.Logging;
using Quaver.States.Gameplay.GameModes;
using Quaver.States.Gameplay.HitObjects;

namespace Quaver.States.Gameplay
{
    internal abstract class GameModeRuleset
    {
        /// <summary>
        ///     The game mode this playfield is for.
        /// </summary>
        internal GameMode Mode { get; set; }

        /// <summary>
        ///     The supervising gameplay screen.
        /// </summary>
        internal GameplayScreen Screen { get; }

        /// <summary>
        ///     The objects in the pool.
        /// </summary>
        internal HitObjectManager HitObjectManager { get; private set; }

        /// <summary>
        ///     Reference to the map.
        /// </summary>
        protected Qua Map { get; }

        /// <summary>
        ///     The playfield for this ruleset.
        /// </summary>
        internal IGameplayPlayfield Playfield { get; set; }

        /// <summary>
        ///     The input manager for this ruleset.
        /// </summary>
        protected abstract IGameplayInputManager InputManager { get; set; }

        /// <summary>
        ///     The score processor for this ruleset.
        /// </summary>
        protected abstract ScoreProcessor ScoreProcessor { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="map"></param>
        internal GameModeRuleset(GameplayScreen screen, Qua map)
        {
            Map = map;
            Screen = screen;
        }
        
        /// <summary>
        ///     Initializes the game mode ruleset.
        /// </summary>
        internal void Initialize()
        {
            // Create and initalize the playfield.
            CreatePlayfield();
            Playfield.Initialize(null);
            
            // Create the input manager after the playfield since the input manager relies on it.
            InputManager = CreateInputManager();

            // Create the HitObjectPool.
            HitObjectManager = CreateHitObjectManager();
            
            // Initialize all HitObjects after creating the pool.
            InitializeHitObjects();
        }

         /// <summary>
        ///     Updates the game mode ruleset.
        /// </summary>
        /// <param name="dt"></param>
        internal void Update(double dt)
        {
            HitObjectManager.Update(dt);
            Playfield.Update(dt);
        }

        /// <summary>
        ///     Draws the game mode.
        /// </summary>
        internal void Draw()
        {
            Playfield.Draw();
        }

        /// <summary>
        ///     Destroys the game mode.
        /// </summary>
        internal void Destroy()
        {
            Playfield.UnloadContent();
        }
        
        /// <summary>
        ///     Initializes all the HitObjects
        /// </summary>
        private void InitializeHitObjects()
        {
            for (var i = 0; i < Map.HitObjects.Count; i++)
            {
                var hitObject = CreateHitObject(Map.HitObjects[i]);

                // Only actually initialize 
                if (i < HitObjectManager.PoolSize)
                    hitObject.InitializeSprite(Playfield);

                // Add this object to the pool.
                HitObjectManager.ObjectPool.Add(hitObject);
            }
            
            Logger.LogInfo($"Initialized HitObjects - " + HitObjectManager.ObjectPool.Count, LogType.Runtime);
        }
        
        /// <summary>
        ///     Handles the input of the game mode.
        /// </summary>
        /// <param name="dt"></param>
        internal void HandleInput(double dt)
        {
            InputManager.HandleInput(dt);
        }
        
        /// <summary>
        ///     Initializes a single HitObject.
        /// </summary>
        protected abstract HitObject CreateHitObject(HitObjectInfo info);
        
        /// <summary>
        ///     Creates the actual playfield.
        /// </summary>
        protected abstract void CreatePlayfield();

        /// <summary>
        ///     Creates the input manager for this ruleset.
        /// </summary>
        protected abstract IGameplayInputManager CreateInputManager();

        /// <summary>
        ///     Creates a custom HitObjectManager for this ruleset.
        /// </summary>
        /// <returns></returns>
        protected abstract HitObjectManager CreateHitObjectManager();
    }
}