using System;
using Quaver.Config;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.Graphics.Buttons
{
    /// <summary>
    ///     Sliders, used to change 
    /// </summary>
    internal class QuaverSlider: QuaverButton
    {
        /// <summary>
        ///     Reference to the value that's changing in the slider.
        /// </summary>
        internal BindedInt BindedValue { get; }

        /// <summary>
        ///     The containing object
        /// </summary>
        private QuaverContainer Container { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - Creates the slider object.
        /// </summary>
        internal QuaverSlider(BindedInt bindedValue)
        {
            BindedValue = bindedValue;
            Held += OnHold;

            Image = FontAwesome.Coffee;
            Alignment = Alignment.MidCenter;
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