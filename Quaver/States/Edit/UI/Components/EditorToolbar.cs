using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Edit.UI.Components
{
    internal class EditorToolbar : Sprite
    {
        /// <summary>
        ///     Reference to the editor screen.
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal EditorToolbar(EditorScreen screen)
        {
            Screen = screen;
            
            Size = new UDim2D(100, GameBase.WindowRectangle.Height);
            Tint = Color.Black;
            Alpha = 0.8f;
        }
    }
}