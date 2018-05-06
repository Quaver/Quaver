using Microsoft.Xna.Framework.Input;
using Quaver.Config;

namespace Quaver.States.Gameplay.GameModes.Keys.Input
{
    internal class KeysInputButtonContainer
    {
        /// <summary>
        ///     The key that this maps to.
        /// </summary>
        internal BindedValue<Microsoft.Xna.Framework.Input.Keys> Key { get; }

        /// <summary>
        ///     If the key is currently pressed.
        /// </summary>
        internal bool Pressed { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="key"></param>
        internal KeysInputButtonContainer(BindedValue<Microsoft.Xna.Framework.Input.Keys> key)
        {
            Key = key;
        }
    }
}