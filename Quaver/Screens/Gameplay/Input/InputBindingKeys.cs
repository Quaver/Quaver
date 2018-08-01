using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Wobble.Bindables;

namespace Quaver.Screens.Gameplay.Input
{
    public class InputBindingKeys
    {
        /// <summary>
        ///     The key that this maps to.
        /// </summary>
        public Bindable<Keys> Key { get; }

        /// <summary>
        ///     If the key is currently pressed.
        /// </summary>
        public bool Pressed { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="key"></param>
        public InputBindingKeys(Bindable<Keys> key) => Key = key;
    }
}
