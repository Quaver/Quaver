﻿using System;
using System.Drawing;
using Microsoft.Xna.Framework.Input;
using Quaver.Graphics.Base;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Buttons
{
    /// <inheritdoc />
    /// <summary>
    /// This class will be inherited from every button class.
    /// </summary>
    internal abstract class QuaverButton : Sprites.QuaverSprite
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
        ///     Determines if the button is currently hovered over.
        /// </summary>
        internal bool IsHovered { get; set; }
        
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
        /// <param name="action"></param>
        protected QuaverButton(EventHandler clickAction = null, EventHandler holdAction = null)
        {
            Clicked += clickAction;
            Held += holdAction;
        }
        
        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            PreviousMouseState = CurrentMouseState;
            CurrentMouseState = Mouse.GetState();
            
            if (GetClickArea())
            {
                IsHovered = true;
                MouseOver();

                // If the user is holding onto the button
                if (CurrentMouseState.LeftButton == ButtonState.Pressed)
                    OnHeld();
                
                // If the user actually clicks the button, fire off the click event.
                if (CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
                    OnClicked();
                
            }
            else
            {
                if (IsHovered)
                {
                    IsHovered = false;
                    MouseOut();
                }
         
                // If the user clicks the button outside of button area.
                if (CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
                    OnClickedOutside();
            }

            base.Update(dt);
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
            base.Destroy();
        }
    }
}
