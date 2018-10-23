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
            // Set References
            Screen = screen;
            Map = map;

            // Initialize Everything
            ScoreProcessor = CreateScoreProcessor(Map);
            CreatePlayfield();
            InputManager = CreateInputManager();
            HitObjectManager = CreateHitObjectManager();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Updates the game ruleset.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            if (!Screen.Failed && !Screen.IsPaused)
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
        ///     Creates the score processor for this ruleset.
        /// </summary>
        /// <returns></returns>
        protected abstract ScoreProcessor CreateScoreProcessor(Qua map);

        /// <summary>
        ///     Creates the playfield for the ruleset.
        /// </summary>
        protected abstract void CreatePlayfield();

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
