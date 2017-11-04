using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Quaver.GameState;

namespace Quaver.Input
{
    internal interface IInputManager
    {
        /// <summary>
        ///     The current state for the specifc input manager
        /// </summary>
        State CurrentState { get; set; }
    }
}
