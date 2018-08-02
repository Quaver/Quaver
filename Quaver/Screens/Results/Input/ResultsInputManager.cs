using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Input;

namespace Quaver.Screens.Results.Input
{
    public class ResultsInputManager
    {
        /// <summary>
        ///     Reference to the parent results screen.
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        public ResultsInputManager(ResultsScreen screen) => Screen = screen;

        /// <summary>
        ///     Handles all the input for the entire results screen.
        /// </summary>
        public void HandleInput()
        {
            var screenView = (ResultsScreenView) Screen.View;

            if (KeyboardManager.IsUniqueKeyPress(Keys.F2))
                Screen.ExportReplay();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                if (!Screen.IsExiting)
                    SkinManager.Skin.SoundBack.CreateChannel().Play();

                Screen.Exit(() => Screen.GoBackToMenu());
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
                screenView.ButtonContainer.ChangeSelected(Direction.Backward);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
                screenView.ButtonContainer.ChangeSelected(Direction.Forward);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                screenView.ButtonContainer.FireButtonEvent();
        }
    }
}
