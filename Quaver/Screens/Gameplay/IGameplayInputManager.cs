using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Screens.Gameplay
{
    public interface IGameplayInputManager
    {
        /// <summary>
        ///     Handles all of the input for the entire ruleset.
        /// </summary>
        /// <param name="dt"></param>
        void HandleInput(double dt);
    }
}
