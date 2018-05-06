using System.ComponentModel;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Input;
using Quaver.Logging;
using Quaver.States.Gameplay.GameModes.Keys.Input;
using Quaver.States.Gameplay.GameModes.Keys.Playfield;
using Quaver.States.Gameplay.HitObjects;

namespace Quaver.States.Gameplay.GameModes.Keys
{
    internal class GameModeKeys : GameModeRuleset
    {
        /// <inheritdoc />
        /// <summary>
        ///     The input manager for this ruleset.
        /// </summary>
        protected sealed override IGameplayInputManager InputManager { get; set; }
        
        /// <inheritdoc />
        /// <summary>
        ///     Ctor - Sets the correct mode, either 4 or 7k.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="map"></param>
        public GameModeKeys(GameplayScreen screen, GameMode mode, Qua map): base(screen, map)
        {
            switch (mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    Mode = mode;
                    break;
                default:
                    throw new InvalidEnumArgumentException("GameModeKeys can only be initialized with GameMode.Keys4 or GameModes.Keys7");
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected override HitObject CreateHitObject(HitObjectInfo info)
        {
            var hitObject = new KeysHitObject(info);       
            return hitObject;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void CreatePlayfield()
        {
            Playfield = new KeysPlayfield(Map);
            Logger.LogSuccess("Playfield Initialized", LogType.Runtime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override IGameplayInputManager CreateInputManager()
        {
            return new KeysInputManager(this, Mode);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override HitObjectPool CreateHitObjectPool() => new KeysHitObjectPool(255);
    }
}