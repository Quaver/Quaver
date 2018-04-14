using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.Graphics.Buttons
{
    internal class QuaverSlider : QuaverButton
    {
        /// <summary>
        ///     The value that this slider binds to.
        /// </summary>
        private BindedInt BindedValue { get; }

        /// <summary>
        ///     The circle sprite for the slider.
        /// </summary>
        private QuaverSprite Circle { get; set; }

        /// <summary>
        ///     If the mouse is held down and hasn't let go yet.
        /// </summary>
        private bool MouseInHoldSequence { get; set; }

        /// <summary>
        ///     The last percentage that the slider has changed the BindedInt to.
        /// </summary>
        private int LastPercentage { get; set; }

        /// <summary>
        ///     The mouse state of the previous frame.
        /// </summary>
        private MouseState PreviousMouseState { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new SliderButton. Takes in a BindedInt as an argument.
        /// </summary>
        /// <param name="binded"></param>
        /// <param name="size"></param>
        /// <param name="lineColor"></param>
        /// <param name="progressColor"></param>
        /// <param name="progressTexture"></param>
        internal QuaverSlider(BindedInt binded, Vector2 size, Color lineColor, Color progressColor, Texture2D progressTexture = null)
        {
            BindedValue = binded;
            Held += MouseHeld;
            
            Size.X.Offset = size.X;
            Size.Y.Offset = size.Y;
            Tint = lineColor;
            
            // Create the progress circle sprite.
            if (progressTexture == null)
                progressTexture = FontAwesome.Circle;
            
            Circle = new QuaverSprite()
            {
                Alignment = Alignment.TopLeft,
                Image = progressTexture,
                Size = new UDim2D(28, 28),
                Tint = progressColor,
                Parent = this
            };
            
            // Set initial position of the slider circle based on the percentage proportion.
            var percentage = BindedValue.Value - BindedValue.MinValue / BindedValue.MaxValue * 100;
            Circle.Position = new UDim2D(percentage / 100f * Size.X.Offset, Size.Y.Offset / 2 - Circle.SizeY / 2, 1, 0);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            if (GameBase.MouseState.LeftButton == ButtonState.Released)
                MouseInHoldSequence = false;

            // Handle the changing of the value for this button.
            if (MouseInHoldSequence)
                HandleSliderValueChanges();

            PreviousMouseState = GameBase.MouseState;
            base.Update(dt);
        }

        /// <summary>
        ///     Gets the new value of the slider and sets it to the binded value.
        /// </summary>
        private void HandleSliderValueChanges()
        {
            // Get the percentage of the mouse position, to the size of the slider.
            var percentage = (int)((GameBase.MouseState.X - AbsolutePosition.X) / AbsoluteSize.X * 100);

            // If the percentage of the MouseX/SliderX is 0% or lower, set the binded value to the minimum.
            if (percentage <= 0 && LastPercentage > 0)
            {
                BindedValue.Value = BindedValue.MinValue;
                Circle.PosX = percentage / 100f * Size.X.Offset;
            }
                
            // If the percentage of the MouseX/SliderX is 100% or higher set the binded value to the maximum.
            else if (percentage >= 100 && LastPercentage < 100)
            {
                BindedValue.Value = BindedValue.MaxValue;
                Circle.PosX = percentage / 100f * Size.X.Offset;
            }
                
            // If the percentage is anything else, set it accordingly.
            else if (percentage > 0 && percentage < 100 && LastPercentage != percentage)
            {
                BindedValue.Value = (int)(percentage / 100f * BindedValue.MaxValue);
                Circle.PosX = percentage / 100f * Size.X.Offset;
            }
            
            LastPercentage = percentage;
        }
        
        /// <summary>
        ///     When the button is moused over and held down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseHeld(object sender, EventArgs e)
        {
            if (PreviousMouseState.LeftButton == ButtonState.Released)
                MouseInHoldSequence = true;   
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     MouseOut
        /// </summary>
        protected override void MouseOver()
        {
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     MouseOut
        /// </summary>
        protected override void MouseOut()
        {
        }
    }
}