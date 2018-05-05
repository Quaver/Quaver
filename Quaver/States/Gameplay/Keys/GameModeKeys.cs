using System.Collections.Generic;
using System.ComponentModel;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Database.Maps;
using Quaver.Graphics.Sprites;

namespace Quaver.States.Gameplay.Keys
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
        /// <param name="objectInfo"></param>
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
        }
    }
}