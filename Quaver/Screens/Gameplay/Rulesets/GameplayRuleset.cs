using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Structures;
using Quaver.Screens.Gameplay.Rulesets.HitObjects;

namespace Quaver.Screens.Gameplay.Rulesets
{
    public abstract class GameplayRuleset : IGameScreenComponent
    {
        /// <summary>
        ///     Reference to the parent gameplay screen
        /// </summary>
        public GameplayScreen Screen { get; }

        /// <summary>
        ///     The map being played
        /// </summary>
        public Qua Map { get; }

        /// <summary>
        ///     The game mode of the map being played.
        /// </summary>
        public GameMode Mode => Map.Mode;

        /// <summary>
        ///     The playfield for this ruleset.
        /// </summary>
        public IGameplayPlayfield Playfield { get; protected set; }

        /// <summary>
        ///     The input manager for this ruleset.
        /// </summary>
        public IGameplayInputManager InputManager { get; protected set; }

        /// <summary>
        ///     Manages all of the HitObjects for the screen.
        /// </summary>
        public HitObjectManager HitObjectManager { get; private set; }

        /// <summary>
        ///     Manages all the scoring for this play session and ruleset.
        /// </summary>
        public ScoreProcessor ScoreProcessor { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="map"></param>
        protected GameplayRuleset(GameplayScreen screen, Qua map)
        {
            Screen = screen;
            Map = map;

            Initialize();
        }

        /// <summary>
        ///     Initializes the ruleset.
        /// </summary>
        private void Initialize()
        {
            ScoreProcessor = CreateScoreProcessor(Map);
            CreatePlayfield();

            InputManager = CreateInputManager();

            HitObjectManager = CreateHitObjectManager();
            InitializeHitObjects();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates the game ruleset.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            if (!Screen.Failed)
                HitObjectManager.Update(gameTime);

            Playfield.Update(gameTime);
        }

        /// <summary>
        ///     Handles the input of the game mode ruleset.
        /// </summary>
        /// <param name="gameTime"></param>
        public void HandleInput(GameTime gameTime) => InputManager.HandleInput(gameTime.ElapsedGameTime.TotalMilliseconds);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime) => Playfield.Draw(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Destroy() => Playfield.Destroy();

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
        }

        /// <summary>
        ///     Creates the score processor for this ruleset.
        /// </summary>
        /// <returns></returns>
        protected abstract ScoreProcessor CreateScoreProcessor(Qua map);

        /// <summary>
        ///     Creates the playfield for the ruleset.
        /// </summary>
        protected abstract void CreatePlayfield();

        /// <summary>
        ///     Initializes a single HitObject.
        /// </summary>
        protected abstract GameplayHitObject CreateHitObject(HitObjectInfo info);

        /// <summary>
        ///     Creates a custom HitObjectManager for this ruleset.
        /// </summary>
        protected abstract HitObjectManager CreateHitObjectManager();

        /// <summary>
        ///     Creates the input manager for the ruleset.
        /// </summary>
        /// <returns></returns>
        protected abstract IGameplayInputManager CreateInputManager();
    }
}
