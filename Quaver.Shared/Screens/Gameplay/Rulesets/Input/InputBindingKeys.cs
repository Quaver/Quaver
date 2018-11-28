using Wobble.Bindables;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Input
{
    public class InputBindingKeys
    {
        /// <summary>
        ///     The key that this maps to.
        /// </summary>
        public Bindable<Microsoft.Xna.Framework.Input.Keys> Key { get; }

        /// <summary>
        ///     If the key is currently pressed.
        /// </summary>
        public bool Pressed { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="key"></param>
        public InputBindingKeys(Bindable<Microsoft.Xna.Framework.Input.Keys> key) => Key = key;
    }
}
