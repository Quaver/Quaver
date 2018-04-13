using System;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Main;

namespace Quaver.Graphics.Buttons.Sliders
{
    internal class SliderButton : QuaverButton
    {
        /// <summary>
        ///     The value that this slider binds to.
        /// </summary>
        private BindedInt BindedValue { get; }

        /// <summary>
        ///     If the mouse is held down and hasn't let go yet.
        /// </summary>
        private bool MouseInHoldSequence { get; set; }

        /// <summary>
        ///     The last percentage that the slider has changed the BindedInt to.
        /// </summary>
        private int LastPercentage { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new SliderButton. Takes in a BindedInt as an argument.
        /// </summary>
        /// <param name="binded"></param>
        internal SliderButton(BindedInt binded)
        {
            BindedValue = binded;
            Held += MouseHeld;
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
            
            base.Update(dt);
        }

        /// <summary>
        ///     Gets the new value of the slider and sets it to the binded value.
        /// </summary>
        private void HandleSliderValueChanges()
        {
            // Get the percentage of the mouse position, to the size of the slider.
            var percentage = (int)((GameBase.MouseState.X - Parent.AbsolutePosition.X) / Parent.AbsoluteSize.X * 100);

            // If the percentage of the MouseX/SliderX is 0% or lower, set the binded value to the minimum.
            if (percentage <= 0 && LastPercentage > 0)
                BindedValue.Value = BindedValue.MinValue;
            // If the percentage of the MouseX/SliderX is 100% or higher set the binded value to the maximum.
            else if (percentage >= 100 && LastPercentage < 100)
                BindedValue.Value = BindedValue.MaxValue;
            // If the percentage is anything else, set it accordingly.
            else if (percentage > 0 && percentage < 100 && LastPercentage != percentage)
                BindedValue.Value = (int)(percentage / 100f * BindedValue.MaxValue);
            
            LastPercentage = percentage;
        }
        
        /// <summary>
        ///     When the button is moused over and held down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseHeld(object sender, EventArgs e)
        {
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