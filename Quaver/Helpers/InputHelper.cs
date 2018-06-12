using Microsoft.Xna.Framework.Input;
using Quaver.Main;

namespace Quaver.Helpers
{
    internal static class InputHelper
    {
        /// <summary>
        ///     Checks for unique key presses.
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        internal static bool IsUniqueKeyPress(Keys k)
        {
            return GameBase.KeyboardState.IsKeyDown(k) && GameBase.PreviousKeyboardState.IsKeyUp(k);
        }

        internal static bool IsUniqueKeyRelease(Keys k)
        {
            return GameBase.KeyboardState.IsKeyUp(k) && GameBase.PreviousKeyboardState.IsKeyDown(k);
        }
    }
}