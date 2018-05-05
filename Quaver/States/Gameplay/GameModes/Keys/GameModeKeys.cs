using System.ComponentModel;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Logging;
using Quaver.States.Gameplay.GameModes.Keys.Playfield;
using Quaver.States.Gameplay.HitObjects;

namespace Quaver.States.Gameplay.GameModes.Keys
{
    internal class GameModeKeys : GameModeRuleset
    {       
         /// <inheritdoc />
        /// <summary>
        ///     Ctor - Sets the correct mode, either 4 or 7k.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="map"></param>
        public GameModeKeys(GameMode mode, Qua map): base(map)
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
         ///     Handles the input of the game mode.
         /// </summary>
         /// <param name="dt"></param>
        internal override void HandleInput(double dt)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected override HitObject CreateHitObject(HitObjectInfo info)
        {
            var hitObject = new KeysHitObject();

            // Initialize Object
            
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
    }
}