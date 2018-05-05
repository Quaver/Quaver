using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Graphics.Sprites;
using Quaver.Logging;
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
        ///     The objects in the pool.
        /// </summary>
        private HitObjectPool HitObjectPool { get; }

        /// <summary>
        ///     Reference to the map.
        /// </summary>
        protected Qua Map { get; }

        /// <summary>
        ///     The playfield for this ruleset.
        /// </summary>
        protected IGameplayPlayfield Playfield { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="map"></param>
        internal GameModeRuleset(Qua map)
        {
            Map = map;
            HitObjectPool = new HitObjectPool(255);
        }
        
        /// <summary>
        ///     Initializes the game mode ruleset.
        /// </summary>
        internal void Initialize()
        {
            // Create and initalize the playfield.
            CreatePlayfield();
            Playfield.Initialize(null);
            
            // Initialize all HitObjects.
            InitializeHitObjects();
        }

         /// <summary>
        ///     Updates the game mode ruleset.
        /// </summary>
        /// <param name="dt"></param>
        internal void Update(double dt)
        {
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
                
                // If the pool isn't full, then initialize the object.
                if (i < HitObjectPool.Size)
                    hitObject.Initialize(Playfield);
                    
                // Add this object to hhe pool.
                HitObjectPool.Objects.Add(hitObject);
            }
            
            Logger.LogInfo($"Initialized HitObjects", LogType.Runtime);
        }
        
        /// <summary>
        ///     Creates the actual playfield.
        /// </summary>
        protected abstract void CreatePlayfield();
        
        /// <summary>
        ///     Handles the input of the game mode.
        /// </summary>
        /// <param name="dt"></param>
        internal abstract void HandleInput(double dt);

        /// <summary>
        ///     Initializes all the HitObjects in the game.
        /// </summary>
        protected abstract HitObject CreateHitObject(HitObjectInfo info);
    }
}