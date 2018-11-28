using Microsoft.Xna.Framework.Input;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Options.Keybindings
{
    public struct KeybindingOptionStore
    {
        /// <summary>
        ///     The name of the keybinding
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The key being set.
        /// </summary>
        public Bindable<Keys> Key { get; }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        public KeybindingOptionStore(string name, Bindable<Keys> key)
        {
            Name = name;
            Key = key;
        }
    }
}