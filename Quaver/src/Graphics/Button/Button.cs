using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Logging;

using Quaver.Utility;

namespace Quaver.Graphics.Button
{
    /// <summary>
    /// This class will be inherited from every button class.
    /// </summary>
    internal abstract class Button : Sprite.Sprite
    {
        internal Button()
        {
            GameBase.GlobalInputManager.LeftClicked += MouseClicked;
        }

        /// <summary>
        ///     Used to detect when the user hovers over the button so the MouseOver() and MouseOut() methods get called only once.
        /// </summary>
        private bool MouseHovered { get; set; }

        /// <summary>
        ///     Determines if the Event Listener will be fired if the button is clicked.
        /// </summary>
        internal bool Clickable { get; set; } = true;

        /// <summary>
        ///     This event handler is used to detect when this object gets clicked. Used externally
        /// </summary>
        internal event EventHandler Clicked;

        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        internal abstract void MouseOver();

        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        internal abstract void MouseOut();

        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            // Check if moouse is over
            var over = Util.RectangleContains(GlobalRectangle, Util.PointToVector2(GameBase.MouseState.Position));

            //Animation
            if (over && !MouseHovered)
            {
                MouseHovered = true;
                MouseOver();
            }
            else if (!over && MouseHovered)
            {
                MouseHovered = false;
                MouseOut();
            }

            base.Update(dt);
        }

        /// <summary>
        ///     This method checks if the mouse has clicked this button specifically
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseClicked(object sender, EventArgs e)
        {
            if (MouseHovered) {
                OnClicked();
            }
        }

        /// <summary>
        ///     This method is called when the button gets clicked
        /// </summary>
        internal void OnClicked()
        {
            if (Clickable) Clicked?.Invoke(this, null);
        }

        internal override void Destroy()
        {
            GameBase.GlobalInputManager.LeftClicked -= MouseClicked;
            base.Destroy();
        }
    }
}
