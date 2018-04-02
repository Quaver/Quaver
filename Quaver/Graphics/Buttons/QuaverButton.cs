using System;
using Quaver.Helpers;

namespace Quaver.Graphics.Buttons
{
    /// <inheritdoc />
    /// <summary>
    /// This class will be inherited from every button class.
    /// </summary>
    internal abstract class QuaverButton : Sprite.Sprite
    {
        internal QuaverButton()
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
            var over = GraphicsHelper.RectangleContains(GlobalRectangle, GraphicsHelper.PointToVector2(GameBase.MouseState.Position));

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
            if (MouseHovered)
                OnClicked();
            else
                OnClickedOutside();
        }

        /// <summary>
        ///     This method is called when the player has clicked outside the button
        /// </summary>
        internal virtual void OnClickedOutside()
        {
            //todo: do stuff
        }

        /// <summary>
        ///     This method is called when the button gets clicked
        /// </summary>
        internal virtual void OnClicked()
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
