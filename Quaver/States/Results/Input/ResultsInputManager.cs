using Microsoft.Xna.Framework.Input;
using Quaver.Helpers;
using Quaver.Input;
using Quaver.Main;
using Quaver.States.Select;

namespace Quaver.States.Results.Input
{
    internal class ResultsInputManager
    {
        /// <summary>
        ///     Reference to the parent results screen.
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal ResultsInputManager(ResultsScreen screen) => Screen = screen;

        /// <summary>
        ///     Handles all the input for the entire results screen.
        /// </summary>
        /// <param name="dt"></param>
        internal void HandleInput(double dt)
        {
            if (InputHelper.IsUniqueKeyPress(Keys.F2))
                Screen.ExportReplay();

            if (InputHelper.IsUniqueKeyPress(Keys.Escape))
                GameBase.GameStateManager.ChangeState(new SongSelectState());
        }
    }
}