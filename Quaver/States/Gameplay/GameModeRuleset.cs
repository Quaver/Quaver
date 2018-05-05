using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Graphics.Sprites;
using Quaver.Logging;

namespace Quaver.States.Gameplay
{
    internal abstract class GameModeRuleset
    {
        /// <summary>
        ///     The game mode this playfield is for.
        /// </summary>
        internal GameMode Mode { get; set; }

        /// <summary>
        ///     The playfield that this game mode will be drawn to.
        /// </summary>
        internal QuaverContainer Playfield { get; set; }

        /// <summary>
        ///     The objects in the pool.
        /// </summary>
        private HitObjectPool HitObjectPool { get; }

        /// <summary>
        ///     Reference to the map.
        /// </summary>
        private Qua Map { get; }

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
        internal virtual void Initialize()
        {
            Playfield = new QuaverContainer();
            CreatePlayfield();
            InitializeHitObjects();
        }

         /// <summary>
        ///     Updates the game mode ruleset.
        /// </summary>
        /// <param name="dt"></param>
        internal virtual void Update(double dt)
        {
            Playfield.Update(dt);    
        }

        /// <summary>
        ///     Draws the game mode.
        /// </summary>
        internal virtual void Draw()
        {
            Playfield.Draw();
        }

        /// <summary>
        ///     Destroys the game mode.
        /// </summary>
        internal virtual void Destroy()
        {
            Playfield.Destroy();
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
    }
}