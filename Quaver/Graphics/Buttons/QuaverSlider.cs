using System;

namespace Quaver.Graphics.Buttons
{
    /// <summary>
    ///     Sliders, used to change 
    /// </summary>
    internal class QuaverSlider : QuaverButton
    {
        /// <inheritdoc />
        /// <summary>
        ///     Ctor - Creates the slider object.
        /// </summary>
        internal QuaverSlider()
        {
            Held += OnHold;
        }
        
        /// <summary>
        ///     Called when holding onto the slider.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnHold(object sender, EventArgs e)
        {
            Console.WriteLine("Slider is being held!");
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     MouseOut
        /// </summary>
        protected override void MouseOut()
        {      
        }

        /// <inheritdoc />
        /// <summary>
        ///     MouseOver
        /// </summary>
        protected override void MouseOver()
        {
        }


    }
}