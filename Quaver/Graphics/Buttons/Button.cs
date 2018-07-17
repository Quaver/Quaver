using System;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Quaver.Graphics.Base;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Buttons
{
    /// <inheritdoc />
    /// <summary>
    /// This class will be inherited from every button class.
    /// </summary>
    internal abstract class Button : Sprite
    {
        /// <summary>
        ///     Event that fires when the button is clicked.
        /// </summary>
        internal EventHandler Clicked;

        /// <summary>
        ///     Event that fires when the button is being held down
        /// </summary>
        internal EventHandler Held;

        /// <summary>
        ///     Called every loop. Useful for custom update methods.
        /// </summary>
        internal Action<double> OnUpdate;

        /// <summary>
        ///     Determines if the button is currently hovered over.
        /// </summary>
        internal bool IsTrulyHovered { get; set; }

        /// <summary>
        ///     If the button is currently hovered over without any layer checks.
        /// </summary>
        private bool IsHoveredWithoutLayerCheck { get; set; }

        /// <summary>
        ///     If the button is actually clickable.
        /// </summary>
        internal bool IsClickable { get; set; } = true;

        /// <summary>
        ///     The mouse state of the previous frame
        /// </summary>
        private MouseState PreviousMouseState { get; set; }

        /// <summary>
        ///     The mouse state of the current frame.
        /// </summary>
        private MouseState CurrentMouseState { get; set; }

        /// <summary>
        ///     Ctor - Optionally pass in an action.
        /// </summary>
        internal Button(EventHandler clickAction = null, EventHandler holdAction = null)
        {
            Clicked += clickAction;
            Held += holdAction;

            ButtonManager.Add(this);
        }

        /// <inheritdoc />
        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();

            if (GetClickArea() && QuaverGame.Game.IsActive && Visible)
            {
                IsHoveredWithoutLayerCheck = true;

                // Check if this button is truly being hovered over by its draw order.
                if (ButtonManager.Buttons.FindAll(x => x.IsHoveredWithoutLayerCheck).OrderByDescending(x => x.DrawOrder).First() != this)
                {
                    IsTrulyHovered = false;
                    MouseOut();
                }
                else
                {
                    IsTrulyHovered = true;
                    MouseOver();

                    // If the user is holding onto the button
                    if (CurrentMouseState.LeftButton == ButtonState.Pressed)
                        OnHeld();

                    // If the user actually clicks the button, fire off the click event.
                    if (CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
                        OnClicked();
                }
            }
            else
            {
                IsHoveredWithoutLayerCheck = false;

                if (IsTrulyHovered)
                {
                    IsTrulyHovered = false;
                    MouseOut();
                }

                // If the user clicks the button outside of button area.
                if (CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
                    OnClickedOutside();
            }

            base.Update(dt);

            // Call custom update method
            OnUpdate?.Invoke(dt);
        }

        /// <summary>
        ///     Method for buttons to get the click area. of the button.
        /// </summary>
        protected virtual bool GetClickArea()
        {
            return GraphicsHelper.RectangleContains(GlobalRectangle, GraphicsHelper.PointToVector2(GameBase.MouseState.Position));
        }

        /// <summary>
        ///     When the user actually clicks the button.
        /// </summary>
        protected virtual void OnClicked()
        {
            if (!IsClickable)
                return;

            Clicked?.Invoke(this, new EventArgs());
        }

        /// <summary>
        ///     When the user is holding down the button
        /// </summary>
        protected virtual void OnHeld()
        {
            Held?.Invoke(this, new EventArgs());
        }

        /// <summary>
        ///     This method is called when the player has clicked outside the button
        ///     Note: No default implementation. Not forced.
        /// </summary>
        protected virtual void OnClickedOutside() {  }

         /// <summary>
        ///     When the mouse cursor leaves the button
        /// </summary>
        protected abstract void MouseOut();

        /// <summary>
        ///     When the mouse cursor enters the area of the button
        /// </summary>
        protected abstract void MouseOver();

        /// <inheritdoc />
        /// <summary>
        ///     Destroys the button. Removes all event handlers.
        /// </summary>
        internal override void Destroy()
        {
            Clicked = null;
            OnUpdate = null;
            Held = null;
            ButtonManager.Remove(this);

            base.Destroy();
        }
    }
}
